/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Editor.Versioning
{
    using System;

    public class ProductInfo
    {
        public string name { get; set; }

        public string generalName { get; set; }

        public Version installedVersion { get; set; }

        public Version newestVersion { get; set; }

        public Version latestPatch { get; set; }

        public ProductType type { get; set; }

        public string storeUrl { get; set; }

        public string productUrl { get; set; }

        public string description { get; set; }

        public string changeLogPath { get; set; }

        public ProductStatus status
        {
            get
            {
                if (this.installedVersion == null)
                {
                    if (string.IsNullOrEmpty(this.storeUrl))
                    {
                        return ProductStatus.ComingSoon;
                    }

                    return ProductStatus.ProductAvailable;
                }

                if (this.newestVersion == null || this.installedVersion >= this.newestVersion)
                {
                    if (this.latestPatch != null && this.latestPatch > this.installedVersion)
                    {
                        return ProductStatus.PatchAvailable;
                    }

                    return ProductStatus.UpToDate;
                }
                else
                {
                    return ProductStatus.UpdateAvailable;
                }
            }
        }

        internal bool isInstalled
        {
            get { return (this.status != ProductStatus.ComingSoon) && (this.status != ProductStatus.ProductAvailable); }
        }

        internal IProductIdentifier product
        {
            get;
            set;
        }

        internal PngResource icon { get; set; }

        internal bool expanded { get; set; }

        internal bool newUpdateAvailable { get; set; }
    }
}
