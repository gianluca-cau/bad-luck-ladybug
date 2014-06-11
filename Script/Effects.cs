using UnityEngine;
using System.Collections;

public class Effects : MonoBehaviour {

	public static IEnumerator MoveAndFade(GameObject obj,Vector2 direction,float speed,float fadeAmountPerFrame)
	{

		while(obj.GetComponent<SpriteRenderer>().color.a > 0)
		{
			obj.SetActive(true);
			obj.transform.position += new Vector3(direction.x,direction.y,0) * speed;
			Color c = obj.GetComponent<SpriteRenderer>().color;
			c.a -= fadeAmountPerFrame;
			obj.GetComponent<SpriteRenderer>().color = c;

			yield return null;
		}
	}

	//draw text of a specified color, with a specified outline color
	
	public static void DrawOutline(Rect position, string text, GUIStyle style, Color outColor, Color inColor,int offset){
		
		//GUIStyle backupStyle = style;
		
		style.normal.textColor = outColor;
		
		position.x -= offset * 0.5f;
		
		GUI.Label(position, text, style);
		
		position.x += offset;
		
		GUI.Label(position, text, style);
		
		position.x -= offset * 0.5f;
		
		position.y-= offset * 0.5f;
		
		GUI.Label(position, text, style);
		
		position.y += offset;
		
		GUI.Label(position, text, style);
		
		position.y -= offset * 0.5f;
		
		style.normal.textColor = inColor;
		
		GUI.Label(position, text, style);
		
		//style = backupStyle;
		
	}
}
