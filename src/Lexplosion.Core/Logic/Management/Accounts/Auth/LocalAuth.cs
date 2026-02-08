using System;
using System.Security.Cryptography;
using System.Text;

namespace Lexplosion.Logic.Management.Accounts.Auth
{
	class LocalAuth : IAuthHandler
	{
		private string ConstructOfflinePlayerUuid(string username)
		{
			string input = "OfflinePlayer:" + username;
			byte[] inputBytes = Encoding.UTF8.GetBytes(input);

			// Вычисляем MD5 хэш
			byte[] hash;
			using (MD5 md5 = MD5.Create())
			{
				hash = md5.ComputeHash(inputBytes);
			}

			hash[6] = (byte)((hash[6] & 0x0F) | 0x30);
			hash[8] = (byte)((hash[8] & 0x3F) | 0x80);

			return BitConverter.ToString(hash).Replace("-", "").ToLower();
		}

		public IAuthHandler.AuthResult Auth(string login, string accessData)
		{
			return new IAuthHandler.AuthResult
			{
				Login = login,
				UUID = ConstructOfflinePlayerUuid(login), // генерируем UUID аналогичный тому, что генерирует сам майнкрафт
				AccessToken = "null",
				SessionToken = null,
				AccessData = null,
				Status = ActivityStatus.Offline
			};
		}

		public IAuthHandler.AuthResult ReAuth(string login, string accessData)
		{
			return Auth(login, accessData);
		}
	}
}
