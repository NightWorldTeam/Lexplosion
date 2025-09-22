namespace Lexplosion.Logic.Management.Addons
{
	class AddonDependencie
	{
		public readonly string AddonId;
		public readonly IPrototypeAddon AddonPrototype;

		public AddonDependencie(string addonId, IPrototypeAddon addonPrototype)
		{
			AddonId = addonId;
			AddonPrototype = addonPrototype;
		}
	}
}
