using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Image_Viewer {
    public class ZoomBorder:Border {
        private UIElement child = null;
        private Point origin;
        private Point start;
        public bool enabled = true;
        public void toggleEnabled() {
            enabled = !enabled;
            releaseMouse();
            Reset();
        }
        private TranslateTransform GetTranslateTransform(UIElement element) {
            return (TranslateTransform)((TransformGroup)element.RenderTransform).Children.First(tr=>tr is TranslateTransform);
        }
        private ScaleTransform GetScaleTransform(UIElement element) {
            return (ScaleTransform)((TransformGroup)element.RenderTransform).Children.First(tr=>tr is ScaleTransform);
        }
        public override UIElement Child {
            get {
                return base.Child;
            }
            set {
                if(value != null && value != Child) {
                    Initialize(value);
                    base.Child = value;
                }
            }
        }
        public void Initialize(UIElement element) {
            child = element;
            TransformGroup group = new TransformGroup();
            ScaleTransform scaleTransform = new ScaleTransform();
            group.Children.Add(scaleTransform);
            TranslateTransform translateTransform = new TranslateTransform();
            group.Children.Add(translateTransform);
            child.RenderTransform = group;
            child.RenderTransformOrigin = new Point(0.0,0.0);
            MouseWheel += child_MouseWheel;
            MouseLeftButtonDown += child_MouseLeftButtonDown;
            MouseLeftButtonUp += child_MouseLeftButtonUp;
            MouseMove += child_MouseMove;
        }
        public void Reset() {
            if(!(child.IsMouseCaptured)) {
                ScaleTransform scaleTransform = GetScaleTransform(child);
                scaleTransform.ScaleX = 1.0;
                scaleTransform.ScaleY = 1.0;
                TranslateTransform translateTransform = GetTranslateTransform(child);
                translateTransform.X = 0.0;
                translateTransform.Y = 0.0;
            }
        }
        public void releaseMouse() {
            child.ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
        }
        public void child_MouseWheel(object sender,MouseWheelEventArgs e) {
            if(!(child.IsMouseCaptured) && enabled) {
                ScaleTransform scaleTransform = GetScaleTransform(child);
                TranslateTransform translateTransform = GetTranslateTransform(child);
                double zoom;
                if(e.Delta > 0) {
                    zoom = 0.5;
                } else {
                    zoom = -0.5;
                }
                if((!(e.Delta < 0) && (scaleTransform.ScaleX > 4.5 || scaleTransform.ScaleY > 4.5)) || (!(e.Delta > 0) && (scaleTransform.ScaleX < 1.0 || scaleTransform.ScaleY < 1.0))) {
                    return;
                }
                Point relative = e.GetPosition(child);
                double abosuluteX = relative.X* scaleTransform.ScaleX + translateTransform.X;
                double abosuluteY = relative.Y * scaleTransform.ScaleY + translateTransform.Y;
                scaleTransform.ScaleX += zoom;
                scaleTransform.ScaleY += zoom;
                translateTransform.X = abosuluteX - relative.X * scaleTransform.ScaleX;
                translateTransform.Y = abosuluteY - relative.Y * scaleTransform.ScaleY;
            }
        }
        private void child_MouseLeftButtonDown(object sender,MouseButtonEventArgs e) {
            if(enabled) {
                TranslateTransform translateTransform = GetTranslateTransform(child);
                start = e.GetPosition(this);
                origin = new Point(translateTransform.X,translateTransform.Y);
                Cursor = Cursors.SizeAll;
                child.CaptureMouse();
            }
        }
        private void child_MouseLeftButtonUp(object sender,MouseButtonEventArgs e) {
            releaseMouse();
        }
        private void child_MouseMove(object sender,MouseEventArgs e) {
            if(child.IsMouseCaptured) {
                TranslateTransform translateTransform = GetTranslateTransform(child);
                Vector vector = start - e.GetPosition(this);
                translateTransform.X = origin.X - vector.X;
                translateTransform.Y = origin.Y - vector.Y;
            }
        }
    }
}