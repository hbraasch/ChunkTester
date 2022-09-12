using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChunkTester
{
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    internal class StartupPage : ContentPage
    {

        public class DisplayChildItem
        {
            public string Text { get; set; }
        }
        public class DisplayGroupItem: List<DisplayChildItem>
        {
            public string Text { get; set; }
        }
        public List<DisplayGroupItem> RawDisplayItems { get; set; } = new();
        public ObservableRangeCollection<DisplayGroupItem> DisplayItems { get; set; } = new();

        public StartupPage()
        {
            BindingContext = this;

            for (int i = 0; i < 10; i++)
            {
                var groupItem = new DisplayGroupItem { Text = $"Group {i}"};
                for (int j = 0; j < 10; j++)
                {
                    groupItem.Add(new DisplayChildItem { Text = $"Child {i}_{j}" });
                }
                RawDisplayItems.Add(groupItem);
            }


            var nextButton = new Button { Text = "Next" };
            nextButton.Clicked += (s, e) => { 
            
            };

            var carouselView = new CollectionView()
            {
                SelectionMode = SelectionMode.Single,
                IsGrouped = true,
                BackgroundColor = Colors.Transparent,
                Margin = 5,
                RemainingItemsThreshold = 5
            };
            carouselView.ItemTemplate = new DataTemplate(() => {

                var label = new Label();
                label.SetBinding(Label.TextProperty, nameof(DisplayChildItem.Text));
                return label;
            });
            carouselView.GroupHeaderTemplate = new DataTemplate(() => {

                var label = new Label();
                label.SetBinding(Label.TextProperty, nameof(DisplayGroupItem.Text));
                return label;
            });

            carouselView.SetBinding(CarouselView.ItemsSourceProperty, new Binding(nameof(DisplayItems),BindingMode.TwoWay,source: this));
            carouselView.RemainingItemsThresholdReached += (s, e) => {
                Debug.WriteLine("Retrieving chunk");
                UpdateDisplay();
            };

            Content = carouselView; 
        }

        bool hasAppearedOnce = false;
        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (!hasAppearedOnce)
            {
                UpdateDisplay();
                hasAppearedOnce = true;
            }
        }

        int chunkCount = 0;
        int chunkSize = 25;
        public void UpdateDisplay()
        {

            var minDisplayAmount = (chunkCount * chunkSize) + chunkSize;
            var displayAmount = 0;
            var groupItemCount = 0;
            foreach (var groupItem in RawDisplayItems)
            {
                groupItemCount += 1;
                displayAmount += groupItem.Count();
                if (displayAmount > minDisplayAmount) break;
            }
            for (int i = DisplayItems.Count; i < groupItemCount; i++)
            {
                DisplayItems.Add(RawDisplayItems[i]);
            }
            chunkCount++;
        }
    }
}
