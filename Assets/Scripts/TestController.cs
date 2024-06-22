using System;
using TMPro;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;

public class TestController : MonoBehaviour
{

	public TextMeshProUGUI text;
	private String textString;
	private bool flag = true;

	private static readonly String responseName = "ADD_USER";

	void Start()
	{
		NetworkingController networking = NetworkManager.getNetworkingController();
		networking.startListenOnMessage(TestUpdated, responseName);
	}

	void OnDestroy()
	{
		NetworkingController networking = NetworkManager.getNetworkingController();
		networking.stopListenOnMessage(TestUpdated, responseName);
	}

	void Update()
	{
		text.text = textString;
		if (flag)
		{
			Request();
			flag = false;
		}

	}
	private void TestUpdated(object sender, MessageEventArgs e)
	{
		Debug.Log("Server says: " + e.Data);

		RoomUserResponse roomUser = RoomUserResponse.CreateFromJSON(e.Data);
		textString = "";
	}
	public void Request()
	{
		NetworkingController networking = NetworkManager.getNetworkingController();
		requestTest(networking);
	}

	private void requestTest(NetworkingController networking)
	{
		var request = new RoomUserRequest(0);
		networking.sendRequest(request);
	}

	public class RoomUserRequest : ClientRequest
	{
		private int roomId;
		public RoomUserRequest(int roomid)
		{
			type = "ADD_USER";
			this.roomId = roomid;
		}
	}

	public class RoomUserResponse
	{
		public string roomState;
		public RoomUser[] rommUsers;
		public static RoomUserResponse CreateFromJSON(string jsonString)
		{
			return JsonConvert.DeserializeObject<RoomUserResponse>(jsonString);
		}
	}
	public class RoomUser
	{
		public string userID;
		public string userName;
		public Position pos;
		public Rotation rot;
		public string roomUserType;

		public RoomUser()
		{

		}
		public class Position
		{
			public float x;
			public float y;
			public float z;
		}
		public class Rotation
		{
			public float x;
			public float y;
			public float z;
		}
	}
	public class TestRequest : ClientRequest
	{
		public int a;
		public int b;
		public int c;

		public TestRequest(int a, int b, int c)
		{
			type = "TEST";
			this.a = a;
			this.b = b;
			this.c = c;
		}
	}

	public class TestResponse
	{
		public int answer;

		public static TestResponse CreateFromJSON(string jsonString)
		{
			return JsonUtility.FromJson<TestResponse>(jsonString);
		}
	}
}
