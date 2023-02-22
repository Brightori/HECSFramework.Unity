using System.Collections;
using System.Collections.Generic;
using HECSFramework.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class UtilsTests
{
    [Test]
    public void HECSListSwapRemoveTest()
    {
        var list = new HECSList<IndexTest>(6);

        for (int i = 0; i < 16; i++)
        {
            list.Add(new IndexTest { Index = i });
        }

        for (int i = 0; i < 4; i++)
        {
            list.RemoveSwap(list.Data[8]);
        }

        bool badRow = false;

        for (int i = 0; i < 12; i++)
        {
            if (list.Data[i] == null)
            {
                badRow = true;
                break;
            }
        }

        Assert.IsTrue(list.Count == 12 && !badRow);
    }

    [Test]
    public void HECSListSwapAtZeroIndex()
    {
        var list = new HECSList<IndexTest>(6);

        for (int i = 0; i < 16; i++)
        {
            list.Add(new IndexTest { Index = i });
        }

        for (int i = 0; i < 16; i++)
        {
            list.RemoveAtSwap(0);
        }

        Assert.IsTrue(list.Count == 0 && list.Data[0] == null);
    }

    public class IndexTest
    {
        public int Index = 0;
    }
}
