//
// MessageBar.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2008 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using Gtk;

using Hyena.Gui;
using Hyena.Data.Gui;

namespace Hyena.Widgets
{
    public class MessageBar : Alignment
    {   
        private HBox box;
        private AnimatedImage image;
        private Label label;
        private Button button;
        private Button close_button;
        
        private ListViewGraphics graphics;
        
        public event EventHandler ButtonClicked {
            add { button.Clicked += value; }
            remove { button.Clicked -= value; }
        }
        
        public event EventHandler CloseClicked {
            add { close_button.Clicked += value; }
            remove { close_button.Clicked -= value; }
        }
        
        public MessageBar () : base (0.0f, 0.5f, 1.0f, 0.0f)
        {
            HBox shell_box = new HBox ();
            shell_box.Spacing = 10;
        
            box = new HBox ();
            box.Spacing = 10;
            
            image = new AnimatedImage ();
            image.Pixbuf = Gtk.IconTheme.Default.LoadIcon ("process-working", 22, IconLookupFlags.NoSvg);
            image.FrameHeight = 22;
            image.FrameWidth = 22;
            Spinning = false;
            image.Load ();
            
            label = new Label ();
            label.Xalign = 0.0f;
            label.Show ();
            
            button = new Button ();
            
            box.PackStart (image, false, false, 0);
            box.PackStart (label, true, true, 0);
            box.Show ();
            
            close_button = new Button (new Image (Stock.Close, IconSize.Menu));
            close_button.Relief = ReliefStyle.None;
            close_button.Clicked += delegate { Hide (); };
            close_button.ShowAll ();
            close_button.Hide ();
            
            shell_box.PackStart (box, true, true, 0);
            shell_box.PackStart (close_button, false, false, 0);
            shell_box.Show ();
            
            Add (shell_box);
            
            EnsureStyle ();
            
            BorderWidth = 3;
        }
        
        protected override void OnRealized ()
        {
            base.OnRealized ();
            
            graphics = new ListViewGraphics (this);
            graphics.RefreshColors ();
        }
        
        protected override bool OnExposeEvent (Gdk.EventExpose evnt)
        {
            if (!IsDrawable) {
                return false;
            }
 
            Cairo.Context cr = Gdk.CairoHelper.Create (evnt.Window);
                
            try {
                Gdk.Color color = Style.Background (StateType.Normal);
                graphics.DrawFrame (cr, Allocation, CairoExtensions.GdkColorToCairoColor (color));
                return base.OnExposeEvent(evnt);
            } finally {
                ((IDisposable)cr.Target).Dispose ();
                ((IDisposable)cr).Dispose ();
            }
        }
        
        private bool changing_style = false;
        protected override void OnStyleSet (Gtk.Style previousStyle)
        {
            if (changing_style) {
                return;
            }
            
            changing_style = true;
            Window win = new Window (WindowType.Popup);
            win.Name = "gtk-tooltips";
            win.EnsureStyle ();
            Style = win.Style;
            changing_style = false;
        }
        
        public string ButtonLabel {
            set {
                bool should_remove = false;
                foreach (Widget child in box.Children) {
                    if (child == button) {
                        should_remove = true;
                        break;
                    }
                }
                
                if (should_remove && value == null) {
                    box.Remove (button);
                }
                
                if (value != null) {
                    button.Label = value;
                    button.Show ();
                    box.PackStart (button, false, false, 0);
                }
                
                QueueDraw ();
            }
        }
        
        public bool ShowCloseButton {
            set {
                close_button.Visible = value;
                QueueDraw ();
            }
        }
        
        public bool ButtonUseStock {
            set {
                button.UseStock = value;
                QueueDraw ();
            }
        }
        
        public string Message {
            set {
                label.Markup = value;
                QueueDraw ();
            }
        }
        
        public Gdk.Pixbuf Pixbuf {
            set {
                image.InactivePixbuf = value;
                image.Visible = value != null || Spinning;
                QueueDraw ();
            }
        }
        
        public bool Spinning {
            get { return image.Active; }
            set { image.Active = value; }
        }
    }
}
