using Doozy.Engine.Themes;
using Doozy.Engine.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.MJRP.UIHelper.DoozieExtensions
{
    [AddComponentMenu("Doozy/Themes/Targets/Color Target Button", MenuUtils.ColorTargetImage_AddComponentMenu_Order)]
    [DefaultExecutionOrder(DoozyExecutionOrder.COLOR_TARGET_IMAGE)]
    public class ColorTargetButton : ThemeTarget
    {

        #region Public Variables

        /// <summary> Target Image component </summary>
        public Button button;

        /// <summary> Determines if the target color preserves its alpha value when the theme variant changes </summary>
        public bool OverrideAlpha;

        /// <summary> Alpha value for the target color when the theme variant changes (when OverrideAlpha is true) </summary>
        public float Alpha;

        public Guid HighlightedID = Guid.Empty;
        [SerializeField]
        private byte[] HighlightedIDSerializedGuid;

        public Guid PressedID = Guid.Empty;
        [SerializeField]
        private byte[] PressedIDSerializedGuid;

        public Guid SelectedID = Guid.Empty;
        [SerializeField]
        private byte[] SelectedIDSerializedGuid;

        #endregion

        #region Private Variables

        private float m_previousAlphaValue = -1;

        #endregion

        #region Unity Methods

        private void Update()
        {
            if (!OverrideAlpha) return;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Alpha == m_previousAlphaValue) return;
            SetAlpha(Alpha);
            m_previousAlphaValue = Alpha;
        }

        public override void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();
            HighlightedIDSerializedGuid = GuidUtils.GuidToSerializedGuid(HighlightedID);
            PressedIDSerializedGuid = GuidUtils.GuidToSerializedGuid(PressedID);
            SelectedIDSerializedGuid = GuidUtils.GuidToSerializedGuid(SelectedID);
        }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            HighlightedID = GuidUtils.SerializedGuidToGuid(HighlightedIDSerializedGuid);
            PressedID = GuidUtils.SerializedGuidToGuid(PressedIDSerializedGuid);
            SelectedID = GuidUtils.SerializedGuidToGuid(SelectedIDSerializedGuid);
        }

        #endregion

        #region Public Methods

        /// <summary> Method used by the ThemeManager when the active variant or selected theme have changed </summary>
        /// <param name="theme"> Target theme </param>
        public override void UpdateTarget(ThemeData theme)
        {
            if (button == null) return;
            if (theme == null) return;
            base.UpdateTarget(theme);
            if (ThemeId == Guid.Empty) return;
            if (PropertyId == Guid.Empty) return;
            if (theme.ActiveVariant == null) return;

            var colors = button.colors;
            colors.normalColor = theme.ActiveVariant.GetColor(PropertyId);
            colors.highlightedColor = theme.ActiveVariant.GetColor(HighlightedID);
            colors.pressedColor = theme.ActiveVariant.GetColor(PressedID);
            colors.selectedColor = theme.ActiveVariant.GetColor(SelectedID);
            button.colors = colors;

            if (!OverrideAlpha) return;
            SetAlpha(Alpha);
        }

        /// <summary> Sets the Alpha value for the target component </summary>
        /// <param name="value"> Alpha value </param>
        public void SetAlpha(float value)
        {
            if (button == null) return;
            Alpha = value;
            Color color = button.targetGraphic.color;
            button.targetGraphic.color = new Color()
            {
                r = color.r,
                g = color.g,
                b = color.b,
                a = Alpha
            }; ;
        }

        #endregion


        #region Private Methods

        private void Reset()
        {
            ThemeId = Guid.Empty;
            VariantId = Guid.Empty;
            PropertyId = Guid.Empty;

            UpdateReference();
        }

        private void UpdateReference()
        {
            if (button == null)
                button = GetComponent<Button>();
        }

        #endregion

    }
}
