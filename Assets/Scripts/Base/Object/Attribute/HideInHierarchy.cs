using System;

namespace ET
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class HideInHierarchy : Attribute
    {
    }
}