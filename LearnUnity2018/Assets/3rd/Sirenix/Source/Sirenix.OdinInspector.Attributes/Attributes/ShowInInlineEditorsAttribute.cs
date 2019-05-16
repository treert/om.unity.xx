//-----------------------------------------------------------------------// <copyright file="ShowInInlineEditorsAttribute.cs" company="Sirenix IVS"> // Copyright (c) Sirenix IVS. All rights reserved.// </copyright>//-----------------------------------------------------------------------
//-----------------------------------------------------------------------
// <copyright file="ShowInInlineEditorsAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// Only shows a property if it is drawn within an <see cref="InlineEditorAttribute"/>.
    /// </summary>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All)]
    public class ShowInInlineEditorsAttribute : Attribute
    {
    }
}