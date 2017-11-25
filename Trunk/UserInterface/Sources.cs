using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UserInterface
{
    public class Sources
    {
        public enum SourceEnum
        {
            MainMenu,
            ControlMaintenance,
            AssetMaintenance
        }

        public enum TableEnum
        {
            Assets,
            AssetTypes,
            Expiry,
            ExpiryStatus,
            ExpiryTypes,
            Manufacturers,
            States
        }
    }
}
