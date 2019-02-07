using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using Nebula_Messenger.Server;

namespace Nebula_Messenger.Source.Pools
{
    public class PoolList : BaseAdapter
    {
        List<Pool> pools = new List<Pool>();

        public void SetPools(List<Pool> _pools)
        {
            pools = _pools;
        }

        public override int Count => pools.Count;

        public Conversation GetConversationObj(int position)
        {
            return new Conversation()
            {
                id = pools[position].poolid,
                title = pools[position].name
            };
        }
        public double[] GetCoordinates(int position)
        {
            return pools[position].coordinates;
        }

        public void Add(Pool pool)
        {
            pools.Add(pool);
        }
        public void RemoveAt(int position)
        {
            pools.RemoveAt(position);
        }
        
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            LinearLayout layout;

            if (convertView == null)
            {
                layout = new LinearLayout(GlobalData.MainActivity)
                {
                    Orientation = Orientation.Vertical
                };
            }
            else
            {
                layout = (LinearLayout)convertView;
                layout.RemoveAllViews();
            }

            TextView nameView = new TextView(GlobalData.MainActivity)
            {
                Text = pools[position].name,
                TextSize = 20
            };
            nameView.SetPadding(50, 20, 50, 20);

            layout.AddView(nameView);

            return layout;
        }

        public override Java.Lang.Object GetItem(int position) { return null; }

        public override long GetItemId(int position) { return position; }
    }
}