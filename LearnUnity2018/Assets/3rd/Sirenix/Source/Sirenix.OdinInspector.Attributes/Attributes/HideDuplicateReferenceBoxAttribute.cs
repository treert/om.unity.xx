//-----------------------------------------------------------------------// <copyright file="HideDuplicateReferenceBoxAttribute.cs" company="Sirenix IVS"> // Copyright (c) Sirenix IVS. All rights reserved.// </copyright>//-----------------------------------------------------------------------
//-----------------------------------------------------------------------
// <copyright file="HideDuplicateReferenceBoxAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// Indicates that Odin should hide the reference box, if this property would otherwise be drawn as a reference to another property, due to duplicate reference values being encountered.
    /// Note that if the value is referencing itself recursively, then the reference box will be drawn regardless of this attribute in all recursive draw calls.
    /// </summary>
    public class HideDuplicateReferenceBoxAttribute : Attribute
    {
    }
}