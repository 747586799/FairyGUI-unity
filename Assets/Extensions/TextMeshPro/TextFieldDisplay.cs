/* 
 * ========================================================
 * 功能描述：
 * 作 者：Programmer Name
 * 创建时间：2023/12/06 16:03:39
 * UnityVersion：2021.3.25f1c1
 * ========================================================
*/
#if FAIRYGUI_TMPRO
namespace FairyGUI
{
    public class TextFieldDisplay : DisplayObject
    {
        public TextFieldDisplay(TextField textField)
        {
            _flags |= Flags.TouchDisabled;

            CreateGameObject("TextFieldDisplay");
            graphics = new NGraphics(gameObject);
            graphics.meshFactory = textField;

            gOwner = textField.gOwner;
        }
    }
}
#endif
