namespace Lexplosion
{
	/// <summary>
	/// Статус онлайн игры
	/// </summary>
	public enum OnlineGameStatus
	{
		None,
		OpenWorld,
		ConnectedToUser
	}

	namespace Logic.Network
	{
		/// <summary>
		/// Состояние системы онлайны игры
		/// </summary>
		public enum SystemState
		{
			Normal,
			ServerNotAvailable
		}
	}
}