using Assets.Scripts.MJRP.UIHelper.DoozieExtensions;
using Doozy.Editor;
using Doozy.Editor.Internal;
using Doozy.Editor.Themes;
using Doozy.Engine.Themes;
using Doozy.Engine.Utils;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using PropertyName = Doozy.Editor.PropertyName;

namespace Assets.Scripts.MJRP_Editor.UIHelper.DoozieExtensions
{
    [CustomEditor(typeof(ColorTargetButton))]
    [CanEditMultipleObjects]
    public class ColorTargetButton_Editor : BaseEditor
    {
        enum GuidSelection
        {
            PROPERTYID,
            HIGHLIGHTID,
            PRESSEDID,
            SELECTEDID
        }

        protected override ColorName ComponentColorName { get { return DGUI.Colors.ThemesColorName; } }

        private ColorTargetButton m_target;

        private ColorTargetButton Target
        {
            get
            {
                if (m_target != null) return m_target;
                m_target = (ColorTargetButton)target;
                return m_target;
            }
        }

        private static ThemesDatabase Database { get { return ThemesSettings.Database; } }
        private string[] ThemesNames;
        private string[] VariantsNames;

        private SerializedProperty
            m_button,
            m_overrideAlpha,
            m_alpha;

        private ThemeData m_theme;

        private bool HasReference { get { return m_button.objectReferenceValue != null; } }

        protected override void LoadSerializedProperty()
        {
            base.LoadSerializedProperty();

            m_button = GetProperty("button");
            m_overrideAlpha = GetProperty(PropertyName.OverrideAlpha);
            m_alpha = GetProperty(PropertyName.Alpha);
        }

        public override void OnInspectorGUI()
        {
            UpdateIds();
            UpdateLists();
            base.OnInspectorGUI();
            serializedObject.Update();
            DrawHeader(Styles.GetStyle(Styles.StyleName.ComponentHeaderColorTargetImage), MenuUtils.ColorTargetImage_Manual, MenuUtils.ColorTargetImage_YouTube);
            GUILayout.Space(DGUI.Properties.Space(2));
            DGUI.Property.Draw(m_button, UILabels.Image, HasReference ? ComponentColorName : ColorName.Red);
            GUILayout.Space(DGUI.Properties.Space());
            ThemeTargetEditorUtils.DrawOverrideAlpha(m_overrideAlpha, m_alpha, Target.button == null ? 1 : Target.button.targetGraphic.color.a, ComponentColorName, InitialGUIColor);
            GUILayout.Space(DGUI.Properties.Space(4));
            int themeIndex = Database.GetThemeIndex(Target.ThemeId);
            if (themeIndex != -1)
            {
                ThemeTargetEditorUtils.DrawThemePopup(Database, m_theme, ThemesNames, themeIndex, ComponentColorName, serializedObject, targets, Target, InitialGUIColor, UpdateIds, UpdateLists);
                GUILayout.Space(DGUI.Properties.Space());
                ThemeTargetEditorUtils.DrawActiveVariant(m_theme, ComponentColorName);
            }

            GUILayout.Space(DGUI.Properties.Space(2));
            GUILayout.Label("Normal Colour");
            int propertyIndex = m_theme.GetColorPropertyIndex(Target.PropertyId);
            if (Target.PropertyId == Guid.Empty || propertyIndex == -1) ThemeTargetEditorUtils.DrawLabelNoPropertyFound();
            else DrawColorProperties(m_theme, propertyIndex, serializedObject, targets, Target, InitialGUIColor, GuidSelection.PROPERTYID);

            GUILayout.Space(DGUI.Properties.Space(2));
            GUILayout.Label("Highlighted Colour");
            int HighlightedID = m_theme.GetColorPropertyIndex(Target.HighlightedID);
            if (Target.HighlightedID == Guid.Empty || HighlightedID == -1) ThemeTargetEditorUtils.DrawLabelNoPropertyFound();
            else DrawColorProperties(m_theme, HighlightedID, serializedObject, targets, Target, InitialGUIColor, GuidSelection.HIGHLIGHTID);

            GUILayout.Space(DGUI.Properties.Space(2));
            GUILayout.Label("Pressed Colour");
            int PressedID = m_theme.GetColorPropertyIndex(Target.PressedID);
            if (Target.PressedID == Guid.Empty || PressedID == -1) ThemeTargetEditorUtils.DrawLabelNoPropertyFound();
            else DrawColorProperties(m_theme, PressedID, serializedObject, targets, Target, InitialGUIColor, GuidSelection.PRESSEDID);

            GUILayout.Space(DGUI.Properties.Space(2));
            GUILayout.Label("Selected Colour");
            int SelectedID = m_theme.GetColorPropertyIndex(Target.SelectedID);
            if (Target.SelectedID == Guid.Empty || SelectedID == -1) ThemeTargetEditorUtils.DrawLabelNoPropertyFound();
            else DrawColorProperties(m_theme, SelectedID, serializedObject, targets, Target, InitialGUIColor, GuidSelection.SELECTEDID);

            GUILayout.Space(DGUI.Properties.Space(4));
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawColorProperties(ThemeData themeData, int propertyIndex,
                                            SerializedObject serializedObject, Object[] targets, ColorTargetButton target,
                                            Color initialGUIColor, GuidSelection guidIndex)
        {
            GUIStyle colorButtonStyle = Styles.GetStyle(Styles.StyleName.ColorButton);
            GUIStyle colorButtonSelectedStyle = Styles.GetStyle(Styles.StyleName.ColorButtonSelected);

            if (themeData.ColorLabels.Count != themeData.ActiveVariant.Colors.Count)
                foreach (LabelId labelId in themeData.ColorLabels.Where(labelId => !themeData.ActiveVariant.ContainsColor(labelId.Id)))
                    themeData.ActiveVariant.AddColorProperty(labelId.Id);

            for (int i = 0; i < themeData.ColorLabels.Count; i++)
            {
                LabelId colorProperty = themeData.ColorLabels[i];
                int index = i;
                bool selected = i == propertyIndex;
                GUILayout.BeginHorizontal();
                {
                    if (!selected) GUILayout.Space((colorButtonSelectedStyle.fixedWidth - colorButtonStyle.fixedWidth) / 2);
                    GUI.color = themeData.ActiveVariant.Colors[i].Color;
                    {
                        if (GUILayout.Button(GUIContent.none, selected ? colorButtonSelectedStyle : colorButtonStyle))
                        {
                            if (serializedObject.isEditingMultipleObjects)
                            {
                                DoozyUtils.UndoRecordObjects(targets, UILabels.UpdateValue);
                                foreach (Object o in targets)
                                {
                                    var themeTarget = (ColorTargetButton)o;
                                    if (themeTarget == null) continue;
                                    switch (guidIndex)
                                    {
                                        case GuidSelection.PROPERTYID:
                                            themeTarget.PropertyId = themeData.ColorLabels[index].Id;
                                            break;
                                        case GuidSelection.HIGHLIGHTID:
                                            themeTarget.HighlightedID = themeData.ColorLabels[index].Id;
                                            break;
                                        case GuidSelection.PRESSEDID:
                                            themeTarget.PressedID = themeData.ColorLabels[index].Id;
                                            break;
                                        case GuidSelection.SELECTEDID:
                                            themeTarget.SelectedID = themeData.ColorLabels[index].Id;
                                            break;
                                    }
                                  
                                    themeTarget.UpdateTarget(themeData);
                                }
                            }
                            else
                            {
                                DoozyUtils.UndoRecordObject(target, UILabels.UpdateValue);
                                switch (guidIndex)
                                {
                                    case GuidSelection.PROPERTYID:
                                        target.PropertyId = themeData.ColorLabels[index].Id;
                                        break;
                                    case GuidSelection.HIGHLIGHTID:
                                        target.HighlightedID = themeData.ColorLabels[index].Id;
                                        break;
                                    case GuidSelection.PRESSEDID:
                                        target.PressedID = themeData.ColorLabels[index].Id;
                                        break;
                                    case GuidSelection.SELECTEDID:
                                        target.SelectedID = themeData.ColorLabels[index].Id;
                                        break;
                                }
                                target.UpdateTarget(themeData);
                            }
                        }
                    }
                    GUI.color = initialGUIColor;
                    GUILayout.Space(DGUI.Properties.Space(2));
                    GUI.enabled = selected;
                    DGUI.Label.Draw(colorProperty.Label, selected ? Size.L : Size.M, selected ? colorButtonSelectedStyle.fixedHeight : colorButtonStyle.fixedHeight);
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(DGUI.Properties.Space());
            }
        }

        private void UpdateIds()
        {
            if (!Database.Contains(Target.ThemeId))
                Target.ThemeId = Database.Themes[0].Id;
            m_theme = Database.GetThemeData(Target.ThemeId);

            if (!m_theme.ContainsColorProperty(Target.PropertyId))
                Target.PropertyId = m_theme.ColorLabels.Count > 0
                                        ? m_theme.ColorLabels[0].Id
                                        : Guid.Empty;

            if (!m_theme.ContainsColorProperty(Target.HighlightedID))
                Target.HighlightedID = m_theme.ColorLabels.Count > 0
                                        ? m_theme.ColorLabels[0].Id
                                        : Guid.Empty;

            if (!m_theme.ContainsColorProperty(Target.PressedID))
                Target.PressedID = m_theme.ColorLabels.Count > 0
                                        ? m_theme.ColorLabels[0].Id
                                        : Guid.Empty;

            if (!m_theme.ContainsColorProperty(Target.SelectedID))
                Target.SelectedID = m_theme.ColorLabels.Count > 0
                                        ? m_theme.ColorLabels[0].Id
                                        : Guid.Empty;
        }


        private void UpdateLists()
        {
            ThemesNames = ThemesDatabase.GetThemesNames(Database);
        }
    }
}