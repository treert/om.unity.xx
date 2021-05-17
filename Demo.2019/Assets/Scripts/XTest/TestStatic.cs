using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace XTest
{
    /// <summary>
    /// 测试发现，每次运行 static TestStatic() 都会调用一次，这么看运行时dll和editor下是两个环境。
    /// </summary>
    public class TestStatic
    {
        static TestStatic()
        {
            Debug.Log(nameof(TestStatic));
        }

        public static void Hello()
        {
            Debug.Log("TestStatic.Hello");
        }
    }

    // 打AB是这行报错。
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public class TestStatic2
    {
        static TestStatic2()
        {
            Debug.Log(nameof(TestStatic2));
        }

        public static void Hello()
        {
            Debug.Log("TestStatic2.Hello");
        }
    }
}
