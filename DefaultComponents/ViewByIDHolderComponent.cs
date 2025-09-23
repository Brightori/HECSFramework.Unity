using System;
using System.Collections.Generic;
using HECSFramework.Core;
using UnityEngine;

namespace Components
{
    [Serializable][Documentation(Doc.HECS, Doc.View, Doc.Visual, "here we can hold views what we use for various visual scenarios")]
    public sealed class IDToViewHolderComponent : BaseComponent
    {
        public HECSList<IDToView> IDToViews = new HECSList<IDToView>(2);
    }

    [Serializable]
    public struct IDToView : IEquatable<IDToView>
    {
        public int ID;
        public GameObject View;

        public IDToView(int iD, GameObject view)
        {
            ID = iD;
            View = view;
        }

        public override bool Equals(object obj)
        {
            return obj is IDToView view &&
                   ID == view.ID &&
                   EqualityComparer<GameObject>.Default.Equals(View, view.View);
        }

        public bool Equals(IDToView other)
        {
            return ID == other.ID &&
                   EqualityComparer<GameObject>.Default.Equals(View, other.View);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, View);
        }
    }
}