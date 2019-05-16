#if UNITY_EDITOR
//-----------------------------------------------------------------------// <copyright file="HideInEditorModeExamples.cs" company="Sirenix IVS"> // Copyright (c) Sirenix IVS. All rights reserved.// </copyright>//-----------------------------------------------------------------------
#pragma warning disable
namespace Sirenix.OdinInspector.Editor.Examples
{
    [AttributeExample(typeof(HideInEditorModeAttribute))]
    internal class HideInEditorModeExamples
    {
        [Title("Hidden in editor mode")]
        [HideInEditorMode]
        public int C;

        [HideInEditorMode]
        public int D;
    }
}
#endif