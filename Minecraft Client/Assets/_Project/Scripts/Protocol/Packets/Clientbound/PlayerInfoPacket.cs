﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerInfoPacket : Packet
{
	public PlayerInfoPacket()
	{
		PacketID = (int)ClientboundIDs.PlayerInfo;
	}

	public PlayerInfoPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

	public Action[] Actions { get; set; }

	public override byte[] Payload
	{
		get => throw new NotImplementedException();
		set
		{
			using (MemoryStream stream = new MemoryStream(value))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					ActionType actionType = (ActionType)PacketReader.ReadVarInt(reader);
					int actionCount = PacketReader.ReadVarInt(reader);
					Actions = new Action[actionCount];

					switch (actionType)
					{
						case ActionType.AddPlayer:
							for (int i = 0; i < Actions.Length; i++)
							{
								var action = new Action()
								{
									ActionType = actionType
								};

								action.UUID = PacketReader.ReadGuid(reader);
								action.Name = PacketReader.ReadString(reader);

								// read property array
								action.Properties = new Property[PacketReader.ReadVarInt(reader)];
								for (int j = 0; j < action.Properties.Length; j++)
								{
									var prop = new Property
									{
										Name = PacketReader.ReadString(reader),
										Value = PacketReader.ReadString(reader),
										Signed = PacketReader.ReadBoolean(reader)
									};

									// signature only exists if Signed = true
									if (prop.Signed)
									{
										prop.Signature = PacketReader.ReadString(reader);
									}
								}

								action.GameMode = (GameMode)PacketReader.ReadVarInt(reader);
								action.Ping = PacketReader.ReadVarInt(reader);
								action.HasDisplayName = PacketReader.ReadBoolean(reader);

								// displayname only exists if HasDisplayName = true
								if (action.HasDisplayName)
								{
									action.DisplayNameJSON = PacketReader.ReadString(reader);
								}

								Actions[i] = action;
							}
							break;
						case ActionType.UpdateGameMode:
							for (int i = 0; i < Actions.Length; i++)
							{
								var action = new Action()
								{
									ActionType = actionType
								};

								action.UUID = PacketReader.ReadGuid(reader);
								action.GameMode = (GameMode)PacketReader.ReadVarInt(reader);

								Actions[i] = action;
							}
							break;
						case ActionType.UpdateLatency:
							for (int i = 0; i < Actions.Length; i++)
							{
								var action = new Action()
								{
									ActionType = actionType
								};

								action.UUID = PacketReader.ReadGuid(reader);
								action.Ping = PacketReader.ReadVarInt(reader);

								Actions[i] = action;
							}
							break;
						case ActionType.UpdateDisplayName:
							for (int i = 0; i < Actions.Length; i++)
							{
								var action = new Action()
								{
									ActionType = actionType
								};

								action.UUID = PacketReader.ReadGuid(reader);
								action.HasDisplayName = PacketReader.ReadBoolean(reader);

								// displayname only exists if HasDisplayName = true
								if (action.HasDisplayName)
								{
									action.DisplayNameJSON = PacketReader.ReadString(reader);
								}

								Actions[i] = action;
							}
							break;
						case ActionType.RemovePlayer:
							for (int i = 0; i < Actions.Length; i++)
							{
								var action = new Action()
								{
									ActionType = actionType
								};

								action.UUID = PacketReader.ReadGuid(reader);

								Actions[i] = action;
							}
							break;
						default:
							throw new Exception($"Unknown PlayerInfo action type: {actionType}");
					}
				}
			}
		}
	}

	/// <summary>
	/// Class for a player update action https://wiki.vg/Protocol#Player_Info
	/// </summary>
	public partial class Action
	{
		public Guid UUID { get; set; }
		public string Name { get; set; }
		public Property[] Properties { get; set; }
		public GameMode GameMode { get; set; }
		public int Ping { get; set; }
		public bool HasDisplayName { get; set; }
		public string DisplayNameJSON { get; set; }
		public ActionType ActionType { get; set; }
	}

	public class Property
	{
		public string Name { get; set; }
		public string Value { get; set; }
		public bool Signed { get; set; }
		public string Signature { get; set; }
	}

	public enum ActionType
	{
		AddPlayer = 0,
		UpdateGameMode = 1,
		UpdateLatency = 2,
		UpdateDisplayName = 3,
		RemovePlayer = 4
	}
}
