package crc64d59c69d44203e9de;


public class MediaService
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("Momo.Droid.MediaService, Momo.Android", MediaService.class, __md_methods);
	}


	public MediaService ()
	{
		super ();
		if (getClass () == MediaService.class)
			mono.android.TypeManager.Activate ("Momo.Droid.MediaService, Momo.Android", "", this, new java.lang.Object[] {  });
	}

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
