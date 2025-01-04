using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.Tools;

namespace Lexplosion.Logic.Management.Addons
{
	internal class InstaceAddonsSynchronizer
	{
		/// <summary>
		/// Тут хранится список аддонов из каталога (метод <see cref="AddonsManager.GetAddonsCatalog"/>). При каждом вызове метода получения каталога этот список обновляется.
		/// Этот кэш необходим чтобы не пересоздавать InstanceClient для зависимого мода, при его скачивании.
		/// </summary>
		public Dictionary<string, InstanceAddon> AddonsCatalogChache { get; private set; }

		/// <summary>
		/// Аддоны, которые устанавливаются в данный момент. После окончания установки они удаляются из этого списка.
		/// Нужно чтобы не создавать новый InstanceClient для тех модов, которые прямо сейчас скачиваются.
		/// Ключ - id мода. Значение - поинтер на InstanceAddon.
		/// </summary>
		public ConcurrentDictionary<string, Pointer<InstanceAddon>> InstallingAddons { get; private set; } = new();
		public KeySemaphore<string> InstallingSemaphore { get; private set; } = new();
		public Semaphore ChacheSemaphore { get; private set; } = new(1, 1);

		/// <summary>
		/// Отвечает за синхронизацию загрузки установленных аддонов
		/// </summary>
		public Semaphore AddonsHandleSemaphore { get; private set; } = new(1, 1);

		public event Action<InstanceAddon> AddonAdded;
		public event Action<InstanceAddon> AddonRemoved;

		/// <summary>
		/// Очищает сохранённый список аддонов. Нужно вызывать при закрытии каталога чтобы очистить память.
		/// </summary>
		public void ClearAddonsListCache()
		{
			Runtime.DebugWrite("Clear chache");
			ChacheSemaphore.WaitOne();
			AddonsCatalogChache = null;
			ChacheSemaphore.Release();
		}

		public void InitAddonsListChache()
		{
			AddonsCatalogChache = new Dictionary<string, InstanceAddon>();
		}

		/// <summary>
		/// Проверяет не устанавливается ли аддон в данный момент
		/// </summary>
		/// <param name="addonId">Айди</param>
		/// <returns>true - устанавливается. Не устанавливается - false</returns>
		public bool CheckAddonInstalling(string addonId)
		{
			try
			{
				InstallingSemaphore.WaitOne(addonId);
				return InstallingAddons.ContainsKey(addonId);
			}
			finally
			{
				InstallingSemaphore.Release(addonId);
			}
		}

		private int _addonsInstalling = 0;
		private event Action _nothingAddonsInstalling;

		private object _addAddonInstallingLocker = new();
		public void AddAddonInstalling(InstanceAddon addon)
		{
			lock (_addAddonInstallingLocker)
			{
                _addonsInstalling++;
            }
		}

		public void RemoveAddonInstalling()
		{
			lock (_addAddonInstallingLocker)
			{
                _addonsInstalling--;
                if (_addonsInstalling == 0)
				{
                    _nothingAddonsInstalling?.Invoke();
                }
			}
		}

		public void Xer(Action action)
		{
			lock (_addAddonInstallingLocker)
			{
				if (_addonsInstalling == 0)
				{
                    action();
                }

				_nothingAddonsInstalling += action;
			}
		}

        private Dictionary<ValueTuple<AddonType, string>, InstanceAddon> _installedAddons = new();
        private object _installedAddonLocker = new();

        public void AddInstalledAddon(InstanceAddon addon)
        {
            lock (_installedAddonLocker)
            {
                _installedAddons[(addon.Type, addon.FileName)] = addon;
                AddonAdded?.Invoke(addon);
            }
        }

        public void AddInstalledAddonWithoutEvent(InstanceAddon addon)
        {
            lock (_installedAddonLocker)
            {
                _installedAddons[(addon.Type, addon.FileName)] = addon;
            }
        }

        public void RemoveInstalledAddon(InstanceAddon addon)
        {
            lock (_installedAddonLocker)
            {
                _installedAddons.Remove((addon.Type, addon.FileName));
                AddonRemoved?.Invoke(addon);
            }
        }

        public bool InstalledAddonContains(InstanceAddon addon)
        {
            lock (_installedAddonLocker)
            {
                return _installedAddons.ContainsKey((addon.Type, addon.FileName));
            }
        }

        public bool InstalledAddonContains(ValueTuple<AddonType, string> addonKey)
        {
            lock (_installedAddonLocker)
            {
                var res = _installedAddons.ContainsKey(addonKey);
				return res;
            }
        }
    }
}
