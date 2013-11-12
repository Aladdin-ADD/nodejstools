﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.NodejsTools.Npm;

namespace Microsoft.NodejsTools.NpmUI{
    internal partial class InstalledPackageListControl : UserControl{
        public InstalledPackageListControl(){
            InitializeComponent();

            //  Hack to get a single auto-sized column
            var header = _listPackages.Columns.Add("Package", "Package");
            header.Width = -2;
            _listPackages.HeaderStyle = ColumnHeaderStyle.None;
            //  /hack

            //  Hack to force the row height
            var images = new ImageList();
            images.ImageSize = new Size(40, 40);
            _listPackages.SmallImageList = images;
            _listPackages.LargeImageList = images;
            //  /hack

            UpdateUIState();
        }

        private void UpdateUIState(){
            _btnUninstall.Enabled = _listPackages.Items.Count > 0 &&
                                    _listPackages.SelectedItems.Count > 0;
        }

        public event EventHandler<PackageEventArgs> UninstallPackageRequested;

        private void OnUninstallPackageRequested(IPackage package){
            var handlers = UninstallPackageRequested;
            if (null != handlers){
                handlers(this, new PackageEventArgs(package));
            }
        }

        public IEnumerable<IPackage> Packages{
            set{
                var source = value ?? new List<IPackage>();
                _listPackages.Items.Clear();
                foreach (var package in source){
                    _listPackages.Items.Add(new ListViewItem(){Tag = package});
                }
            }
        }

        private void _listPackages_SelectedIndexChanged(
            object sender,
            EventArgs e){
            UpdateUIState();
        }

        private void _btnUninstall_Click(object sender, EventArgs e){
            var selected = _listPackages.SelectedItems;
            if (selected.Count > 0){
                foreach (ListViewItem item in selected){
                    OnUninstallPackageRequested(item.Tag as IPackage);
                }
            }
        }

        private void _listPackages_DrawItem(
            object sender,
            DrawListViewItemEventArgs e){
            PackageListItemPainter.DrawItem(this, e);
        }
    }
}