using Android.Support.V4.App;
using Java.Lang;
using System.Collections.Generic;

namespace Nebula_Messenger
{
    internal class ViewPagerAdapter : FragmentPagerAdapter
    {
        List<TitledFragment> fragments;

        public ViewPagerAdapter(FragmentManager manager, List<TitledFragment> _fragments): base(manager)
        {
            fragments = _fragments;
        }

        public override int Count => fragments.Count;

        public TitledFragment GetAt(int position)
        {
            return fragments[position];
        }

        public override ICharSequence GetPageTitleFormatted(int position)
        {
            return new String(fragments[position].Title);
        }

        public override Fragment GetItem(int position)
        {
            return fragments[position];
        }
    }
}