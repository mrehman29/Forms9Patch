using System;
using Xamarin.Forms;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Forms9Patch
{
    public class FlyoutPopup : PopupBase
    {
        #region Properties

        #region Orientation property
        /// <summary>
        /// Backing store for the orientation property.
        /// </summary>
        public static readonly BindableProperty OrientationProperty = BindableProperty.Create("Orientation", typeof(StackOrientation), typeof(FlyoutPopup), StackOrientation.Horizontal);
        /// <summary>
        /// Gets or sets the orientation of the flyout (along which axis does the fly out action occur?).
        /// </summary>
        /// <value>The orientation.</value>
        public StackOrientation Orientation
        {
            get => (StackOrientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }
        #endregion Orientation property

        #region Alignment property
        /// <summary>
        /// Backing store for the alignment property.
        /// </summary>
        public static readonly BindableProperty AlignmentProperty = BindableProperty.Create("Alignment", typeof(FlyoutAlignment), typeof(FlyoutPopup), FlyoutAlignment.Start);
        /// <summary>
        /// Gets or sets the alignment of the flyout along the Orientation axis
        /// </summary>
        /// <value>The alignment.</value>
        public FlyoutAlignment Alignment
        {
            get => (FlyoutAlignment)GetValue(AlignmentProperty);
            set => SetValue(AlignmentProperty, value);
        }
        #endregion Alignment property

        #region Content
        /// <summary>
        /// Gets or sets the content of the FormsPopup.Modal.
        /// </summary>
        /// <value>The content.</value>
        public new View Content
        {
            get => _frame.Content;
            set => _frame.Content = value;
        }
        #endregion

        #endregion

        #region Fields
        Frame _frame;
        #endregion


        #region Constructor / Destructor
        void Init()
        {
            _frame = new Forms9Patch.Frame
            {
                Padding = Padding,
                HasShadow = HasShadow,
                OutlineColor = OutlineColor,
                OutlineWidth = OutlineWidth,
                OutlineRadius = 0,
                BackgroundColor = BackgroundColor
            };
            Margin = 0;
            DecorativeContainerView = _frame;
            UpdateBaseLayoutProperties();
        }

        public FlyoutPopup(bool retain = false, TimeSpan popAfter = default) : base(retain: retain, popAfter: popAfter) => Init();

        public FlyoutPopup(TimeSpan popAfter) : base(popAfter: popAfter) => Init();
        #endregion


        #region Property change management
        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == OrientationProperty.PropertyName || propertyName == AlignmentProperty.PropertyName)
                UpdateBaseLayoutProperties();
        }

        void UpdateBaseLayoutProperties()
        {
            var animation = new Rg.Plugins.Popup.Animations.MoveAnimation();
            animation.PositionIn = animation.PositionOut = Orientation == StackOrientation.Horizontal
                ? Alignment == FlyoutAlignment.Start
                    ? Rg.Plugins.Popup.Enums.MoveAnimationOptions.Left
                    : Rg.Plugins.Popup.Enums.MoveAnimationOptions.Right
                : Alignment == FlyoutAlignment.Start
                    ? Rg.Plugins.Popup.Enums.MoveAnimationOptions.Top
                    : Rg.Plugins.Popup.Enums.MoveAnimationOptions.Bottom;
            Animation = animation;
        }
        #endregion


        #region Layout
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            if (_frame?.Content == null)
                return;


            // layout the page overlay
            //base.LayoutChildren(x, y, width, height);

            height -= KeyboardService.Height;

            // layout the content
            if (width > 0 && height > 0)
            {
                VerticalOptions = Orientation == StackOrientation.Horizontal
                    ? LayoutOptions.Fill
                    : Alignment == FlyoutAlignment.Start
                        ? LayoutOptions.Start
                        : LayoutOptions.End;
                HorizontalOptions = Orientation == StackOrientation.Vertical
                    ? LayoutOptions.Fill
                    : Alignment == FlyoutAlignment.Start
                        ? LayoutOptions.Start
                        : LayoutOptions.End;

                var left = Orientation == StackOrientation.Horizontal && Alignment == FlyoutAlignment.Start
                    ? Padding.Left + Math.Max(SystemPadding.Left - Margin.Left, 0)
                    : Padding.Left;
                var right = Orientation == StackOrientation.Horizontal && Alignment == FlyoutAlignment.End
                    ? Padding.Right + Math.Max(SystemPadding.Right - Margin.Right, 0)
                    : Padding.Right;
                var top = Orientation == StackOrientation.Vertical && Alignment == FlyoutAlignment.End
                    ? Padding.Top
                    : Padding.Top + Math.Max(SystemPadding.Top - Margin.Top, 0);
                var bottom = Orientation == StackOrientation.Vertical && Alignment == FlyoutAlignment.Start
                    ? Padding.Bottom
                    : Padding.Bottom + Math.Max(SystemPadding.Bottom - Margin.Bottom, 0);
                _frame.Padding = new Thickness(left, top, right, bottom);

                _frame.IsVisible = true;
                _frame.Content.IsVisible = true;
                //ShapeBase.UpdateBasePadding(_frame, true);
                //var shadow = ShadowPadding();



                var availWidth = width - (Margin.HorizontalThickness + _frame.Padding.HorizontalThickness); // + shadow.HorizontalThickness);
                var availHeight = height - (Margin.VerticalThickness + _frame.Padding.VerticalThickness); // + shadow.VerticalThickness);

                if (Orientation == StackOrientation.Horizontal)
                    availWidth = _frame.Content.WidthRequest > 0 ? _frame.Content.WidthRequest : 200;
                if (Orientation == StackOrientation.Vertical)
                    availHeight = _frame.Content.HeightRequest > 0 ? _frame.Content.HeightRequest : 200;
                var request = _frame.Content.Measure(availWidth, availHeight, MeasureFlags.None);  //

                var shadowPadding = ShapeBase.ShadowPadding(_frame);

                var rBoxWidth = HorizontalOptions.Alignment == LayoutAlignment.Fill
                    ? availWidth
                    : Math.Min(request.Request.Width, availWidth);// + _frame.Padding.HorizontalThickness);// + shadow.HorizontalThickness);
                rBoxWidth += _frame.Padding.HorizontalThickness;
                rBoxWidth += shadowPadding.HorizontalThickness;
                var rBoxHeight = VerticalOptions.Alignment == LayoutAlignment.Fill
                    ? availHeight
                    : Math.Min(request.Request.Height, availHeight);// + _frame.Padding.VerticalThickness);// + shadow.VerticalThickness);
                rBoxHeight += _frame.Padding.VerticalThickness;
                rBoxHeight += shadowPadding.VerticalThickness;
                var rboxSize = new Size(rBoxWidth, rBoxHeight);

                double contentX = Orientation == StackOrientation.Horizontal
                    ? Alignment == FlyoutAlignment.Start
                        ? -shadowPadding.Left + Margin.Left
                        : width + shadowPadding.Right - rboxSize.Width - Margin.Right
                    : -shadowPadding.Left + Margin.Left;

                double contentY = Orientation == StackOrientation.Vertical
                    ? Alignment == FlyoutAlignment.Start
                        ? -shadowPadding.Top + Margin.Top
                        : height + shadowPadding.Bottom - rboxSize.Height - Margin.Bottom
                    : -shadowPadding.Top + Margin.Top;

                var bounds = new Rectangle(contentX, contentY, rboxSize.Width, rboxSize.Height);
                //System.Diagnostics.Debug.WriteLine("LayoutChildIntoBoundingRegion("+contentX+","+contentY+","+rboxSize.Width+","+rboxSize.Height+")");

                Xamarin.Forms.Layout.LayoutChildIntoBoundingRegion(_frame, bounds);

                _lastLayout = DateTime.Now;

            }
        }
        #endregion


        #region Animation
        protected override void OnAppearingAnimationBegin()
        {

            base.OnAppearingAnimationBegin();
        }

        protected override void OnAppearingAnimationEnd()
        {
            base.OnAppearingAnimationEnd();
        }

        protected override void OnDisappearingAnimationBegin()
        {
            base.OnDisappearingAnimationBegin();
        }

        protected override Task OnAppearingAnimationEndAsync()
        {
            return base.OnAppearingAnimationEndAsync();
        }
        #endregion
    }
}