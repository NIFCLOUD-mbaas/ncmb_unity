package com.nifty.cloud.mb.ncmbgcmplugin;

/**
 * NCMBDialogPushConfiguration is used to setting of dialog push notification
 * 
 */
public class NCMBDialogPushConfiguration {
	/**
	 * display format nothing to display
	 */
	public static final int DIALOG_DISPLAY_NONE = 0x00;
	/**
	 * display format that display dialog
	 */
	public static final int DIALOG_DISPLAY_DIALOG = 0x01;
	/**
	 * display format that display dialog with original background image
	 */
	public static final int DIALOG_DISPLAY_BACKGROUND = 0x02;
	/**
	 * display format that display original layout dialog
	 */
	public static final int DIALOG_DISPLAY_ORIGINAL = 0x04;

	// display format
	private int displayType;

	/**
	 * Costructor<br>
	 * default display formati is DIALOG_DISPLAY_NONE <br>
	 *
	 */
	public NCMBDialogPushConfiguration(){
		//デフォルト非表示
		this.displayType = DIALOG_DISPLAY_NONE;
	}

	/**
	 * Constructor <br>
	 *
	 * @param displayType display format
	 * @param filePath path of background image
	 */
	public NCMBDialogPushConfiguration(int displayType, String filePath){
		this.displayType = displayType;
	}

	/**
	 * set the display format
	 *
	 * @param displayType setting of display format
	 */
	public void setDisplayType(int displayType){
		this.displayType = displayType;
	}

	/**
	 * get the dislay format setting
	 *
	 * @return curernt display format
	 */
	public int getDisplayType(){
		return this.displayType;
	}

}
