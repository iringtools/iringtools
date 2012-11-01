using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System;
using System.ComponentModel;

namespace DbDictionaryEditor.Behaviors
{
    public enum ScrollOrientations
    {
        Auto = 0,
        None = 1,
        Vertical = 2,
        Horizontal = 3
    }

    public class MouseWheelScrollBehavior : Behavior<Control>
    {

        private AutomationPeer Peer { get; set; }

        protected override void OnAttached()
        {
            base.OnAttached();
            this.Peer = FrameworkElementAutomationPeer.FromElement(this.AssociatedObject);

            if (this.Peer == null)
                this.Peer = FrameworkElementAutomationPeer.CreatePeerForElement(this.AssociatedObject);

            this.AssociatedObject.MouseWheel += new MouseWheelEventHandler(AssociatedObject_MouseWheel);
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.MouseWheel -= new MouseWheelEventHandler(AssociatedObject_MouseWheel);
            base.OnDetaching();
        }
        void AssociatedObject_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            this.AssociatedObject.Focus();
            int direction = Math.Sign(e.Delta);
            ScrollAmount scrollAmount = (direction < 0) ? ScrollAmount.SmallIncrement : ScrollAmount.SmallDecrement;

            if (this.Peer != null)
            {
                IScrollProvider scrollProvider =
                    this.Peer.GetPattern(PatternInterface.Scroll) as IScrollProvider;
                bool shiftKey = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;

                if (scrollProvider != null && scrollProvider.VerticallyScrollable && !shiftKey)
                    scrollProvider.Scroll(ScrollAmount.NoAmount, scrollAmount);
                else if (scrollProvider != null && scrollProvider.VerticallyScrollable && shiftKey)
                    scrollProvider.Scroll(scrollAmount, ScrollAmount.NoAmount);

            } 
        }

    }
}
