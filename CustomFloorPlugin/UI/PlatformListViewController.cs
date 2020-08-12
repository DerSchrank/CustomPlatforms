using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace CustomFloorPlugin
{
    internal class PlatformListViewController : BSMLResourceViewController
    {
        public override string ResourceName => "CustomFloorPlugin.UI.PlatformList.bsml";

        public static PlatformListViewController Instance;

        public Action<CustomPlatform> customPlatformChanged;

        [UIComponent("platformList")]
        public CustomListTableData customListTableData;

        [UIAction("platformSelect")]
        public void Select(TableView _, int row)
        {
            PlatformManager.Instance.ChangeToPlatform(row);
            customPlatformChanged?.Invoke(PlatformManager.Instance.currentPlatform);
        }

        [UIAction("reloadPlatforms")]
        public void ReloadMaterials()
        {
            PlatformManager.Instance.RefreshPlatforms();
            SetupList();
            Select(customListTableData.tableView, PlatformManager.Instance.currentPlatformIndex);
        }

        [UIAction("#post-parse")]
        public void SetupList()
        {
            customListTableData.data.Clear();
            foreach (var platform in PlatformManager.Instance.GetPlatforms())
            {
                var customCellInfo = new CustomListTableData.CustomCellInfo(platform.platName, platform.platAuthor, platform.icon?.texture);
                customListTableData.data.Add(customCellInfo);
            }

            customListTableData.tableView.ReloadData();
            var selectedSaber = PlatformManager.Instance.currentPlatformIndex;

            customListTableData.tableView.SelectCellWithIdx(selectedSaber);
            if (!customListTableData.tableView.visibleCells.Where(x => x.selected).Any())
                customListTableData.tableView.ScrollToCellWithIdx(selectedSaber, TableViewScroller.ScrollPositionType.Beginning, true);
        }

        protected override void DidActivate(bool firstActivation, ActivationType type)
        {
            base.DidActivate(firstActivation, type);

            Instance = this;

            Select(customListTableData.tableView, PlatformManager.Instance.currentPlatformIndex);
        }
    }
}