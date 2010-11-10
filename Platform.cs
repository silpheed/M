using System;

namespace m
{
	public class Platform : IPlatform
	{
		
		public bool IsWindows
		{
			get {
				int p = (int) Environment.OSVersion.Platform;
				return ((p != 4) && (p != 6) && (p != 128));
			}
		}
		
		public bool IsUnix
		{
			get { 
				return !IsWindows;
			}
		}
		
		public bool IsMono
		{
			get { 
				return (Type.GetType("Mono.Runtime") != null);
			}
		}
		
		public bool IsDotNet
		{
			get { 
				return !IsMono;
			}
		}
	}
}