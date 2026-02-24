using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine.Internal;

namespace UnityEngine;

public class WWWForm
{
	private List<byte[]> formData;

	private List<string> fieldNames;

	private List<string> fileNames;

	private List<string> types;

	private byte[] boundary;

	private bool containsFiles = false;

	private static byte[] dDash = DefaultEncoding.GetBytes("--");

	private static byte[] crlf = DefaultEncoding.GetBytes("\r\n");

	private static byte[] contentTypeHeader = DefaultEncoding.GetBytes("Content-Type: ");

	private static byte[] dispositionHeader = DefaultEncoding.GetBytes("Content-disposition: form-data; name=\"");

	private static byte[] endQuote = DefaultEncoding.GetBytes("\"");

	private static byte[] fileNameField = DefaultEncoding.GetBytes("; filename=\"");

	private static byte[] ampersand = DefaultEncoding.GetBytes("&");

	private static byte[] equal = DefaultEncoding.GetBytes("=");

	internal static Encoding DefaultEncoding => Encoding.ASCII;

	public Dictionary<string, string> headers
	{
		get
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			if (containsFiles)
			{
				dictionary["Content-Type"] = "multipart/form-data; boundary=\"" + Encoding.UTF8.GetString(boundary, 0, boundary.Length) + "\"";
			}
			else
			{
				dictionary["Content-Type"] = "application/x-www-form-urlencoded";
			}
			return dictionary;
		}
	}

	public byte[] data
	{
		get
		{
			using MemoryStream memoryStream = new MemoryStream(1024);
			if (containsFiles)
			{
				for (int i = 0; i < formData.Count; i++)
				{
					memoryStream.Write(crlf, 0, crlf.Length);
					memoryStream.Write(dDash, 0, dDash.Length);
					memoryStream.Write(boundary, 0, boundary.Length);
					memoryStream.Write(crlf, 0, crlf.Length);
					memoryStream.Write(contentTypeHeader, 0, contentTypeHeader.Length);
					byte[] bytes = Encoding.UTF8.GetBytes(types[i]);
					memoryStream.Write(bytes, 0, bytes.Length);
					memoryStream.Write(crlf, 0, crlf.Length);
					memoryStream.Write(dispositionHeader, 0, dispositionHeader.Length);
					string headerName = Encoding.UTF8.HeaderName;
					string text = fieldNames[i];
					if (!WWWTranscoder.SevenBitClean(text, Encoding.UTF8) || text.IndexOf("=?") > -1)
					{
						text = "=?" + headerName + "?Q?" + WWWTranscoder.QPEncode(text, Encoding.UTF8) + "?=";
					}
					byte[] bytes2 = Encoding.UTF8.GetBytes(text);
					memoryStream.Write(bytes2, 0, bytes2.Length);
					memoryStream.Write(endQuote, 0, endQuote.Length);
					if (fileNames[i] != null)
					{
						string text2 = fileNames[i];
						if (!WWWTranscoder.SevenBitClean(text2, Encoding.UTF8) || text2.IndexOf("=?") > -1)
						{
							text2 = "=?" + headerName + "?Q?" + WWWTranscoder.QPEncode(text2, Encoding.UTF8) + "?=";
						}
						byte[] bytes3 = Encoding.UTF8.GetBytes(text2);
						memoryStream.Write(fileNameField, 0, fileNameField.Length);
						memoryStream.Write(bytes3, 0, bytes3.Length);
						memoryStream.Write(endQuote, 0, endQuote.Length);
					}
					memoryStream.Write(crlf, 0, crlf.Length);
					memoryStream.Write(crlf, 0, crlf.Length);
					byte[] array = formData[i];
					memoryStream.Write(array, 0, array.Length);
				}
				memoryStream.Write(crlf, 0, crlf.Length);
				memoryStream.Write(dDash, 0, dDash.Length);
				memoryStream.Write(boundary, 0, boundary.Length);
				memoryStream.Write(dDash, 0, dDash.Length);
				memoryStream.Write(crlf, 0, crlf.Length);
			}
			else
			{
				for (int j = 0; j < formData.Count; j++)
				{
					byte[] array2 = WWWTranscoder.DataEncode(Encoding.UTF8.GetBytes(fieldNames[j]));
					byte[] toEncode = formData[j];
					byte[] array3 = WWWTranscoder.DataEncode(toEncode);
					if (j > 0)
					{
						memoryStream.Write(ampersand, 0, ampersand.Length);
					}
					memoryStream.Write(array2, 0, array2.Length);
					memoryStream.Write(equal, 0, equal.Length);
					memoryStream.Write(array3, 0, array3.Length);
				}
			}
			return memoryStream.ToArray();
		}
	}

	public WWWForm()
	{
		formData = new List<byte[]>();
		fieldNames = new List<string>();
		fileNames = new List<string>();
		types = new List<string>();
		boundary = new byte[40];
		for (int i = 0; i < 40; i++)
		{
			int num = Random.Range(48, 110);
			if (num > 57)
			{
				num += 7;
			}
			if (num > 90)
			{
				num += 6;
			}
			boundary[i] = (byte)num;
		}
	}

	public void AddField(string fieldName, string value)
	{
		AddField(fieldName, value, Encoding.UTF8);
	}

	public void AddField(string fieldName, string value, Encoding e)
	{
		fieldNames.Add(fieldName);
		fileNames.Add(null);
		formData.Add(e.GetBytes(value));
		types.Add("text/plain; charset=\"" + e.WebName + "\"");
	}

	public void AddField(string fieldName, int i)
	{
		AddField(fieldName, i.ToString());
	}

	[ExcludeFromDocs]
	public void AddBinaryData(string fieldName, byte[] contents)
	{
		AddBinaryData(fieldName, contents, null, null);
	}

	[ExcludeFromDocs]
	public void AddBinaryData(string fieldName, byte[] contents, string fileName)
	{
		AddBinaryData(fieldName, contents, fileName, null);
	}

	public void AddBinaryData(string fieldName, byte[] contents, [DefaultValue("null")] string fileName, [DefaultValue("null")] string mimeType)
	{
		containsFiles = true;
		bool flag = contents.Length > 8 && contents[0] == 137 && contents[1] == 80 && contents[2] == 78 && contents[3] == 71 && contents[4] == 13 && contents[5] == 10 && contents[6] == 26 && contents[7] == 10;
		if (fileName == null)
		{
			fileName = fieldName + (flag ? ".png" : ".dat");
		}
		if (mimeType == null)
		{
			mimeType = ((!flag) ? "application/octet-stream" : "image/png");
		}
		fieldNames.Add(fieldName);
		fileNames.Add(fileName);
		formData.Add(contents);
		types.Add(mimeType);
	}
}
