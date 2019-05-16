#if UNITY_EDITOR
//-----------------------------------------------------------------------// <copyright file="TypeSearchIndex.cs" company="Sirenix IVS"> // Copyright (c) Sirenix IVS. All rights reserved.// </copyright>//-----------------------------------------------------------------------
//-----------------------------------------------------------------------
// <copyright file="TypeSearchIndex.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public sealed class TypeSearchIndex
    {
        public string MatchedTypeLogName = "matched type";

        public List<TypeMatchIndexingRule> IndexingRules = new List<TypeMatchIndexingRule>();
        public List<TypeMatchRule> MatchRules = new List<TypeMatchRule>();
        public Action<string, TypeSearchInfo> LogInvalidTypeInfo = (message, info) => Debug.LogError(message);

        private readonly List<TypeSearchInfo> indexedTypes = new List<TypeSearchInfo>();
        private readonly Type[] CachedTargetArray1 = new Type[1];
        private readonly Type[] CachedTargetArray2 = new Type[2];

        private class TypeArrayEqualityComparer : IEqualityComparer<Type[]>
        {
            public bool Equals(Type[] x, Type[] y)
            {
                if (x == y) return true;
                if (x == null || y == null) return false;
                if (x.Length != y.Length) return false;

                for (int i = 0; i < x.Length; i++)
                {
                    if (x[i] != y[i]) return false;
                }

                return true;
            }

            public int GetHashCode(Type[] obj)
            {
                if (obj == null) return 0;
                const int prime = 137;
                int result = 1;
                unchecked
                {
                    for (int i = 0; i < obj.Length; i++)
                    {
                        var type = obj[i];
                        var typeHash = type == null ? 1 : type.GetHashCode();
                        result = prime * result + (typeHash ^ (typeHash >> 16));
                    }
                }
                return result;
            }
        }

        private Dictionary<Type[], TypeSearchResult[]> resultCache = new Dictionary<Type[], TypeSearchResult[]>(new TypeArrayEqualityComparer());

        public TypeSearchIndex(bool addDefaultValidationRules = true, bool addDefaultMatchRules = true)
        {
            if (addDefaultValidationRules)
            {
                this.AddDefaultIndexingRules();
            }

            if (addDefaultMatchRules)
            {
                this.AddDefaultMatchRules();
            }
        }

        private struct QueryResult
        {
            public int CurrentIndex;
            public double CurrentPriority;
            public TypeSearchResult[] Result;

            public QueryResult(TypeSearchResult[] result)
            {
                this.Result = result;
                this.CurrentIndex = 0;
                this.CurrentPriority = result[0].MatchedInfo.Priority;
            }
        }

        private static readonly List<QueryResult> CachedQueryResultList = new List<QueryResult>();

        //public static IEnumerable<TypeSearchResult> EnumerateMergedQueryResults(List<TypeSearchResult[]> results)
        //{
        //    var queries = CachedQueryResultList;
        //    queries.Clear();

        //    for (int i = 0; i < results.Count; i++)
        //    {
        //        if (results[i].Length == 0)
        //        {
        //            continue;
        //        }

        //        queries.Add(new QueryResult(results[i]));
        //    }

        //    // The below loop is about unifying all the queries (which are individually cached and sorted
        //    // by the TypeSearchIndex) so that their results are combined in a proper sorted order.
        //    //
        //    // The sorted results are temporarily stored in the finalResults list, so that we don't have
        //    // to yield out of this loop while it's running and so introduce unknown performance hits or
        //    // cache misses.

        //    var queriesCount = queries.Count;

        //    while (true)
        //    {
        //        // Find the next drawer with the highest priority
        //        double highestPriority = double.MinValue;
        //        int highestIndex = -1;

        //        for (int i = 0; i < queriesCount; i++)
        //        {
        //            var query = queries[i];
        //            if (query.CurrentIndex >= query.Result.Length) continue;
        //            if (query.CurrentPriority > highestPriority)
        //            {
        //                highestPriority = query.CurrentPriority;
        //                highestIndex = i;
        //            }
        //        }

        //        // If there was no drawer available at all, then we are done.
        //        if (highestIndex == -1)
        //            break;

        //        var highest = queries[highestIndex];
        //        yield return highest.Result[highest.CurrentIndex];
        //        highest.CurrentIndex++;

        //        if (highest.CurrentIndex < highest.Result.Length)
        //        {
        //            highest.CurrentPriority = highest.Result[highest.CurrentIndex].MatchedInfo.Priority;
        //        }

        //        queries[highestIndex] = highest;
        //    }
        //}

        private class MergeSignatureComparer : IEqualityComparer<MergeSignature>
        {
            public bool Equals(MergeSignature x, MergeSignature y)
            {
                if (x.Hash != y.Hash) return false;
                if (object.ReferenceEquals(x.Results, y.Results)) return true;

                var count = x.Results.Count;
                if (count != y.Results.Count) return false;

                for (int i = 0; i < count; i++)
                {
                    if (!object.ReferenceEquals(x.Results[i], y.Results[i])) return false;
                }

                return true;
            }

            public int GetHashCode(MergeSignature obj)
            {
                return obj.Hash;
            }
        }

        private struct MergeSignature
        {
            public int Hash;
            public List<TypeSearchResult[]> Results;

            public MergeSignature(List<TypeSearchResult[]> results)
            {
                this.Results = results;

                const int prime = 137;
                int result = 1;
                unchecked
                {
                    for (int i = 0; i < results.Count; i++)
                    {
                        var hash = results[i].GetHashCode();
                        result = prime * result + (hash ^ (hash >> 16));
                    }
                }
                this.Hash = result;
            }

            public override int GetHashCode()
            {
                return this.Hash;
            }
        }

        private static readonly Dictionary<MergeSignature, TypeSearchResult[]> KnownMergeSignatures = new Dictionary<MergeSignature, TypeSearchResult[]>(new MergeSignatureComparer());
        private static readonly List<TypeSearchResult> CachedFastMergeList = new List<TypeSearchResult>();
        private static readonly TypeSearchResult[] EmptyResultArray = new TypeSearchResult[0];

        public static TypeSearchResult[] GetCachedMergedQueryResults(List<TypeSearchResult[]> results)
        {
            if (results.Count == 0)
            {
                return EmptyResultArray;
            }

            if (results.Count == 1)
            {
                return results[0];
            }

            TypeSearchResult[] fastResultArray;

            // The following merge signature-based caching logic results in a roughly 2-3x speedup over doing the actual merge, once the actual merge has been done once
            var mergeSignature = new MergeSignature(results);

            if (KnownMergeSignatures.TryGetValue(mergeSignature, out fastResultArray))
            {
                return fastResultArray;
            }

            // None of our fast paths worked, so we have to do the actual merging now

            var mergeIntoList = CachedFastMergeList;
            mergeIntoList.Clear();

            var queries = CachedQueryResultList;
            queries.Clear();

            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].Length == 0)
                {
                    continue;
                }

                queries.Add(new QueryResult(results[i]));
            }

            // The below loop is about unifying all the queries (which are individually cached and sorted
            // by the TypeSearchIndex) so that their results are combined in a proper sorted order.
            //
            // The sorted results are temporarily stored in the finalResults list, so that we don't have
            // to yield out of this loop while it's running and so introduce unknown performance hits or
            // cache misses.

            var queriesCount = queries.Count;

            while (true)
            {
                // Find the next drawer with the highest priority
                double highestPriority = double.MinValue;
                int highestIndex = -1;

                for (int i = 0; i < queriesCount; i++)
                {
                    var query = queries[i];
                    if (query.CurrentIndex >= query.Result.Length) continue;
                    if (query.CurrentPriority > highestPriority)
                    {
                        highestPriority = query.CurrentPriority;
                        highestIndex = i;
                    }
                }

                // If there was no drawer available at all, then we are done.
                if (highestIndex == -1)
                    break;

                var highest = queries[highestIndex];
                mergeIntoList.Add(highest.Result[highest.CurrentIndex]);
                highest.CurrentIndex++;

                if (highest.CurrentIndex < highest.Result.Length)
                {
                    highest.CurrentPriority = highest.Result[highest.CurrentIndex].MatchedInfo.Priority;
                }

                queries[highestIndex] = highest;
            }

            mergeSignature.Results = new List<TypeSearchResult[]>(mergeSignature.Results);

            var arr = mergeIntoList.ToArray();
            KnownMergeSignatures.Add(mergeSignature, arr);
            return arr;
        }

        public static void MergeQueryResultsIntoList(List<TypeSearchResult[]> results, List<TypeSearchResult> mergeIntoList)
        {
            mergeIntoList.Clear();

            if (results.Count == 0)
            {
                return;
            }

            if (results.Count == 1)
            {
                var arr = results[0];

                for (int i = 0; i < arr.Length; i++)
                {
                    mergeIntoList.Add(arr[i]);
                }

                return;
            }

            TypeSearchResult[] fastResultArray;

            // The following merge signature-based caching logic results in a roughly 2-3x speedup over doing the actual merge, once the actual merge has been done once
            var mergeSignature = new MergeSignature(results);

            if (KnownMergeSignatures.TryGetValue(mergeSignature, out fastResultArray))
            {
                for (int i = 0; i < fastResultArray.Length; i++)
                {
                    mergeIntoList.Add(fastResultArray[i]);
                }

                return;
            }

            // None of our fast paths worked, so we have to do the actual merging now
            var queries = CachedQueryResultList;
            queries.Clear();

            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].Length == 0)
                {
                    continue;
                }

                queries.Add(new QueryResult(results[i]));
            }

            // The below loop is about unifying all the queries (which are individually cached and sorted
            // by the TypeSearchIndex) so that their results are combined in a proper sorted order.
            //
            // The sorted results are temporarily stored in the finalResults list, so that we don't have
            // to yield out of this loop while it's running and so introduce unknown performance hits or
            // cache misses.

            var queriesCount = queries.Count;

            while (true)
            {
                // Find the next drawer with the highest priority
                double highestPriority = double.MinValue;
                int highestIndex = -1;

                for (int i = 0; i < queriesCount; i++)
                {
                    var query = queries[i];
                    if (query.CurrentIndex >= query.Result.Length) continue;
                    if (query.CurrentPriority > highestPriority)
                    {
                        highestPriority = query.CurrentPriority;
                        highestIndex = i;
                    }
                }

                // If there was no drawer available at all, then we are done.
                if (highestIndex == -1)
                    break;

                var highest = queries[highestIndex];
                mergeIntoList.Add(highest.Result[highest.CurrentIndex]);
                highest.CurrentIndex++;

                if (highest.CurrentIndex < highest.Result.Length)
                {
                    highest.CurrentPriority = highest.Result[highest.CurrentIndex].MatchedInfo.Priority;
                }

                queries[highestIndex] = highest;
            }

            mergeSignature.Results = new List<TypeSearchResult[]>(mergeSignature.Results);
            KnownMergeSignatures.Add(mergeSignature, mergeIntoList.ToArray());
        }

        public void AddIndexedTypes(IEnumerable<TypeSearchInfo> typesToIndex)
        {
            this.indexedTypes.AddRange(
                typesToIndex
                .Select(this.ProcessInfo)
                .Where(n => n != null)
                .Select(n => n.Value)
                .ToArray());

            this.resultCache.Clear();
        }

        public void ClearResultCache()
        {
            this.resultCache.Clear();
        }

        public TypeSearchResult[] GetMatches(Type target)
        {
            if (target == null) throw new ArgumentNullException("target");

            TypeSearchResult[] result;

            var array = CachedTargetArray1;
            array[0] = target;

            if (!this.resultCache.TryGetValue(array, out result))
            {
                var targets = new Type[] { target };

                result = this.FindAllMatches(targets);
                this.resultCache[targets] = result;
            }

            return result;
        }

        public TypeSearchResult[] GetMatches(Type target1, Type target2)
        {
            if (target1 == null) throw new ArgumentNullException("target1");
            if (target2 == null) throw new ArgumentNullException("target2");

            TypeSearchResult[] result;

            var array = CachedTargetArray2;
            array[0] = target1;
            array[1] = target2;

            if (!this.resultCache.TryGetValue(array, out result))
            {
                var targets = new Type[] { target1, target2 };

                result = this.FindAllMatches(targets);
                this.resultCache[targets] = result;
            }

            return result;
        }

        public TypeSearchResult[] GetMatches(params Type[] targets)
        {
            if (targets == null) throw new ArgumentNullException("targets");

            TypeSearchResult[] result;

            if (!this.resultCache.TryGetValue(targets, out result))
            {
                result = this.FindAllMatches(targets);
                this.resultCache[targets] = result;
            }

            return result;
        }

        private TypeSearchResult[] FindAllMatches(Type[] targets)
        {
            List<TypeSearchResult> matches = new List<TypeSearchResult>();

            for (int i = 0; i < this.indexedTypes.Count; i++)
            {
                var info = this.indexedTypes[i];

                if (targets.Length != info.Targets.Length) continue;

                for (int j = 0; j < this.MatchRules.Count; j++)
                {
                    var rule = this.MatchRules[j];
                    bool stopMatchingForInfo = false;
                    var match = rule.Match(info, targets, ref stopMatchingForInfo);

                    if (match != null)
                    {
                        matches.Add(new TypeSearchResult()
                        {
                            MatchedInfo = info,
                            MatchedRule = rule,
                            MatchedType = match,
                            MatchedTargets = targets,
                        });
                        break;
                    }

                    if (stopMatchingForInfo)
                    {
                        break;
                    }
                }
            }

            return matches
                .OrderByDescending(n => n.MatchedInfo.Priority)
                .ToArray();
        }

        private TypeSearchInfo? ProcessInfo(TypeSearchInfo info)
        {
            var originalInfo = info;
            if (info.Targets == null) info.Targets = Type.EmptyTypes;

            for (int i = 0; i < info.Targets.Length; i++)
            {
                if (info.Targets[i] == null)
                {
                    throw new ArgumentNullException("Target at index " + i + " in info for match type " + info.MatchType.GetNiceFullName() + " is null.");
                }
            }

            for (int i = 0; i < this.IndexingRules.Count; i++)
            {
                var rule = this.IndexingRules[i];
                string errorMessage = null;

                if (!rule.Process(ref info, ref errorMessage))
                {
                    if (this.LogInvalidTypeInfo != null)
                    {
                        if (errorMessage == null)
                        {
                            this.LogInvalidTypeInfo("Invalid " + this.MatchedTypeLogName + " declaration '" + originalInfo.MatchType.GetNiceFullName() + "'! Rule '" + rule.Name.Replace("{name}", this.MatchedTypeLogName) + "' failed.", originalInfo);
                        }
                        else
                        {
                            errorMessage = errorMessage.Replace("{name}", this.MatchedTypeLogName);
                            this.LogInvalidTypeInfo("Invalid " + this.MatchedTypeLogName + " declaration '" + originalInfo.MatchType.GetNiceFullName() + "'! Rule '" + rule.Name.Replace("{name}", this.MatchedTypeLogName) + "' failed with message: " + errorMessage, originalInfo);
                        }
                    }

                    return null;
                }
            }

            return info;
        }

        public void AddDefaultMatchRules()
        {
            this.MatchRules.Add(DefaultMatchRules.ExactMatch);
            this.MatchRules.Add(DefaultMatchRules.GenericSingleTargetMatch);
            this.MatchRules.Add(DefaultMatchRules.TargetsSatisfyGenericParameterConstraints);
            this.MatchRules.Add(DefaultMatchRules.GenericParameterInference);
            this.MatchRules.Add(DefaultMatchRules.NestedInSameGenericType);
        }

        public void AddDefaultIndexingRules()
        {
            this.IndexingRules.Add(DefaultIndexingRules.MustBeAbleToInstantiateType);
            this.IndexingRules.Add(DefaultIndexingRules.NoAbstractOrInterfaceTargets);
            this.IndexingRules.Add(DefaultIndexingRules.GenericMatchTypeValidation);
            this.IndexingRules.Add(DefaultIndexingRules.GenericDefinitionSanityCheck);
        }
    }
}
#endif