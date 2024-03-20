/* 
 * ========================================================
 * 功能描述：
 * 作 者：Programmer Name
 * 创建时间：2023/12/06 16:03:39
 * UnityVersion：2021.3.25f1c1
 * ========================================================
*/

#if FAIRYGUI_TMPRO
using System.Collections.Generic;
using FairyGUI.Utils;
using TMPro;
using UnityEngine;

namespace FairyGUI
{
    public class TMPSubTextField : DisplayObject, IMeshFactory
    {
        public enum SubMeshDrawType : byte
        {
            Char = 0,
            Underline = 1,
            Strikethrough = 2,
        }

        public struct CharInfo
        {
            public SubMeshDrawType type;
            /// <summary>
            /// 绘制字符的信息
            /// </summary>
            public TMP_Character character;
            /// <summary>
            /// 绘制line的信息
            /// </summary>
            public float width;
            public int fontSize;

            public float px;
            public float py;
            /// <summary>
            /// 字符占用的顶点数量。
            /// </summary>
            public short vertCount;
            public int charIndex;

            public CharInfo(TMP_Character character, float px, float py, int charIndex)
            {
                type = SubMeshDrawType.Char;
                this.character = character;
                width = 0;
                fontSize = 0;
                this.px = px;
                this.py = py;
                vertCount = 0;
                this.charIndex = charIndex;
            }

            public CharInfo(SubMeshDrawType type, float px, float py, float width, int fontSize)
            {
                this.type = type;
                character = null;
                this.width = width;
                this.fontSize = fontSize;
                this.px = px;
                this.py = py;
                vertCount = 0;
                this.charIndex = 0;
            }
        }

        private TextField _textField;
        private TMPFont _font { set; get; }
        public TMPFont font
        {
            get => _font;
            set
            {
                if (_font == value) return;
                _font = value;
                graphics.SetShaderAndTexture(_font.shader, _font.GetTexture(TextureIndex));
            }
        }

        private List<CharInfo> _toRendererChars = new List<CharInfo>();
        public int TextureIndex;
        public List<CharInfo> toRendererChars => _toRendererChars;

        public TMPSubTextField(TextField textField, int textureIndex)
        {
            TextureIndex = textureIndex;
            _textField = textField;
            _flags |= Flags.TouchDisabled;
            CreateGameObject("SubTextField");
            graphics = new NGraphics(gameObject);
            graphics.meshFactory = this;
            gOwner = textField.gOwner;
        }

        public void OnPopulateMesh(VertexBuffer vb)
        {
            if (_font == null)
            {
                return;
            }

            List<Vector3> vertList = vb.vertices;
            List<Vector2> uvList = vb.uvs;
            List<Vector2> uv2List = vb.uvs2;
            List<Color32> colList = vb.colors;
            int elementIndex = 0;
            _font.SetFormat(_textField.textFormat, _textField._fontSizeScale);
            _font.UpdateGraphics(graphics);
            List<HtmlElement> elements = _textField._elements;
            int elementCount = elements.Count;
            HtmlElement element = null;
            if (elementCount > 0)
                element = _textField._elements[elementIndex];
            for (int i = 0, charCount = _toRendererChars.Count; i < charCount; i++)
            {
                var charInfo = _toRendererChars[i];
                while (elementIndex <= elementCount - 1)
                {
                    element = elements[elementIndex];
                    if (elementIndex == elementCount - 1 || charInfo.charIndex >= elements[elementIndex].charIndex && charInfo.charIndex < elements[elementIndex + 1].charIndex)
                    {
                        break;
                    }
                    else
                    {
                        elementIndex++;
                    }
                }

                if (element != null)
                {
                    _font.SetFormat(element.format, _textField._fontSizeScale);
                }

                if (charInfo.type == SubMeshDrawType.Char)
                    charInfo.vertCount = (short) _font.DrawGlyph(charInfo.character, charInfo.px, charInfo.py, vertList, uvList, uv2List, colList);
                else
                    charInfo.vertCount = (short) _font.DrawLine(charInfo.px, charInfo.py, charInfo.width, charInfo.fontSize,
                        charInfo.type == SubMeshDrawType.Underline ? 0 : 1, vertList, uvList, uv2List, colList);
                _toRendererChars[i] = charInfo;
            }

            int count = vertList.Count;
            if (count > 65000)
            {
                Debug.LogWarning("Text is too large. A mesh may not have more than 65000 vertices.");
                vertList.RemoveRange(65000, count - 65000);
                colList.RemoveRange(65000, count - 65000);
                uvList.RemoveRange(65000, count - 65000);
                if (uv2List.Count > 0)
                    uv2List.RemoveRange(65000, count - 65000);
                count = 65000;
            }

            vb.AddTriangles();
        }

        public int AddToRendererChar(TMP_Character ch, float px, float py, int charIndex)
        {
            int index = _toRendererChars.Count;
            _toRendererChars.Add(new CharInfo(ch, px, py, charIndex));
            return index;
        }

        public int AddToRendererLine(int lineType, float px, float py, float width, int fontSize)
        {
            int index = _toRendererChars.Count;
            _toRendererChars.Add(new CharInfo(lineType == 0 ? SubMeshDrawType.Underline : SubMeshDrawType.Strikethrough, px, py, width, fontSize));
            return index;
        }

        public bool ForceUpdateMesh()
        {
            graphics.SetMeshDirty();
            return graphics.UpdateMesh();
        }

        public void CleanUp()
        {
            _toRendererChars.Clear();
            _font = null;
        }

        public void Clear()
        {
            CleanUp();
            graphics.mesh.Clear();
            graphics.SetMeshDirty();
        }
    }
}

#endif