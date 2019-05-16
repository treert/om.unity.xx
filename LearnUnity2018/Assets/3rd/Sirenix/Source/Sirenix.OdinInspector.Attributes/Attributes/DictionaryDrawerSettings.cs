//-----------------------------------------------------------------------// <copyright file="DictionaryDrawerSettings.cs" company="Sirenix IVS"> // Copyright (c) Sirenix IVS. All rights reserved.// </copyright>//-----------------------------------------------------------------------
//-----------------------------------------------------------------------
// <copyright file="DictionaryDrawerSettings.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// Customize the behavior for dictionaries in the inspector.
    /// </summary>
    public sealed class DictionaryDrawerSettings : Attribute
    {
        /// <summary>
        /// Specify an alternative key label for the dictionary drawer.
        /// </summary>
        public string KeyLabel = "Key";

        /// <summary>
        /// Specify an alternative value label for the dictionary drawer.
        /// </summary>
        public string ValueLabel = "Value";

        /// <summary>
        /// Specify how the dictionary should draw its items.
        /// </summary>
        public DictionaryDisplayOptions DisplayMode;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is read only.
        /// </summary>
        public bool IsReadOnly;
    }
}