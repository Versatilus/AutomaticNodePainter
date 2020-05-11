using ColossalFramework.UI;
using AutomaticNodePainter.Tool;
using AutomaticNodePainter.Util;
using System;
using System.Linq;
using UnityEngine;
using static AutomaticNodePainter.Util.HelpersExtensions;

/* A lot of copy-pasting from Crossings mod by Spectra and Roundabout Mod by Strad. The sprites are partly copied as well. */

namespace AutomaticNodePainter.GUI {
    public class AutomaticNodePainterButton : UIButton {
        public static AutomaticNodePainterButton Instace { get; private set;}

        public static string AtlasName = "AutomaticNodePainterButtonUI_rev" +
            typeof(AutomaticNodePainterButton).Assembly.GetName().Version.Revision;
        const int SIZE = 31;
        const string CONTAINING_PANEL_NAME = "RoadsOptionPanel";
        readonly static Vector2 RELATIVE_POSITION = new Vector3(94, 38);

        const string ButtonBg = "ButtonBg";
        const string ButtonBgActive = "ButtonBgFocused";
        const string ButtonBgHovered = "ButtonBgHovered";
        internal const string Icon = "Icon";
        internal const string IconActive = "IconPressed";

        static UIComponent GetContainingPanel() {
            var ret = GUI.UIUtils.Instance.FindComponent<UIComponent>(CONTAINING_PANEL_NAME, null, GUI.UIUtils.FindOptions.NameContains);
            Log.Debug("GetPanel returns " + ret);
            return ret ?? throw new Exception("Could not find " + CONTAINING_PANEL_NAME);
        }

        public override void Awake() {
            base.Awake();
            Log.Debug("AutomaticNodePainterButton.Awake() is called." + Environment.StackTrace);
        }

        public override void Start() {
            base.Start();
            Log.Info("AutomaticNodePainterButton.Start() is called.");

            name = "AutomaticNodePainterButton";
            playAudioEvents = true;
            tooltip = "Node Controller";

            var builtinTabstrip = GUI.UIUtils.Instance.FindComponent<UITabstrip>("ToolMode", GetContainingPanel(), GUI.UIUtils.FindOptions.None);
            AssertNotNull(builtinTabstrip, "builtinTabstrip");

            UIButton tabButton = (UIButton)builtinTabstrip.tabs[0];

            string[] spriteNames = new string[]
            {
                ButtonBg,
                ButtonBgActive,
                ButtonBgHovered,
                Icon,
                IconActive
            };

            var atlas = TextureUtil.GetAtlas(AtlasName);
            if (atlas == UIView.GetAView().defaultAtlas) {
                atlas = TextureUtil.CreateTextureAtlas("sprites.png", AtlasName, tabButton.atlas.material, SIZE, SIZE, spriteNames);
            }

            Log.Debug("atlas name is: " + atlas.name);
            this.atlas = atlas;

            Activate();
            hoveredBgSprite = ButtonBgHovered;


            relativePosition = RELATIVE_POSITION;
            size = new Vector2(SIZE, SIZE); 
            Show();
            Log.Info("AutomaticNodePainterButton created sucessfully.");
            if (Instace != null) {
                Destroy(Instace); // destroy old instance after cloning
            }
            Instace = this;
        }

        public void Activate() {
            focusedFgSprite = normalBgSprite = pressedBgSprite = disabledBgSprite = ButtonBgActive;
            normalFgSprite = focusedFgSprite = IconActive;
            Invalidate();
        }

        public void Dectivate() {
            focusedFgSprite = normalBgSprite = pressedBgSprite = disabledBgSprite = ButtonBg;
            normalFgSprite = focusedFgSprite = Icon;
            Invalidate();
        }


        public static AutomaticNodePainterButton CreateButton() { 
            Log.Info("AutomaticNodePainterButton.CreateButton() called");
            return GetContainingPanel().AddUIComponent<AutomaticNodePainterButton>();
        }

        protected override void OnClick(UIMouseEventParameter p) {
            Log.Debug("ON CLICK CALLED" + Environment.StackTrace);
            var buttons = UIUtils.GetCompenentsWithName<UIComponent>(name);
            Log.Debug(buttons.ToSTR());

            base.OnClick(p);
            AutomaticNodePainterTool.Instance.ToggleTool();
        }

        public override void OnDestroy() {
            base.OnDestroy();
        }

        public override string ToString() => $"AutomaticNodePainterButton:|name={name} parent={parent.name}|";


    }
}
