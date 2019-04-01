namespace IntegrationTests
{
	using Microsoft.AspNetCore.Identity;
	using Microsoft.AspNetCore.Identity.MongoDB;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.DependencyInjection.Extensions;
	using MongoDB.Driver;
	using NUnit.Framework;
	using System;

	public class UserIntegrationTestsBase : AssertionHelper
	{
		protected MongoDatabase Database;
		protected MongoCollection<IdentityUser> Users;
		protected MongoCollection<IdentityRole> Roles;

		// note: for now we'll have interfaces to both the new and old apis for MongoDB, that way we don't have to update all the tests at once and risk introducing bugs
		protected IMongoDatabase DatabaseNewApi;
		protected IServiceProvider ServiceProvider;
		private readonly string _TestingConnectionString = $"mongodb://10.20.30.106:27017/{IdentityTesting}";
		private const string IdentityTesting = "identity-testing";

		[SetUp]
		public void BeforeEachTest()
		{
			var client = new MongoClient(_TestingConnectionString);

			// todo move away from GetServer which could be deprecated at some point
			Database = client.GetServer().GetDatabase(IdentityTesting);
			Users = Database.GetCollection<IdentityUser>("users");
			Roles = Database.GetCollection<IdentityRole>("roles");

			DatabaseNewApi = client.GetDatabase(IdentityTesting);

			Database.DropCollection("users");
			Database.DropCollection("roles");

			ServiceProvider = CreateServiceProvider<IdentityUser>();
		}

		protected UserManager<IdentityUser> GetUserManager()
			=> ServiceProvider.GetService<UserManager<IdentityUser>>();

		protected RoleManager<IdentityRole> GetRoleManager()
			=> ServiceProvider.GetService<RoleManager<IdentityRole>>();

		protected IServiceProvider CreateServiceProvider<TUser>(Action<IdentityOptions> optionsProvider = null)
			where TUser : IdentityUser
		{
			var services = new ServiceCollection();
			optionsProvider = optionsProvider ?? (options => { });

			services.TryAddScoped<IUserValidator<TUser>, UserValidator<TUser>>();
			services.TryAddScoped<IPasswordValidator<TUser>, PasswordValidator<TUser>>();
			services.TryAddScoped<IPasswordHasher<TUser>, PasswordHasher<TUser>>();
			services.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
			// No interface for the error describer so we can add errors without rev'ing the interface
			services.TryAddScoped<IdentityErrorDescriber>();
			services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<TUser>>();
			services.TryAddScoped<IUserClaimsPrincipalFactory<TUser>, UserClaimsPrincipalFactory<TUser>>();
			services.TryAddScoped<UserManager<TUser>>();
			services.TryAddScoped<SignInManager<TUser>>();

			var idBuilder = new IdentityBuilder(typeof(TUser), services);

			idBuilder
				.AddDefaultTokenProviders()
				.RegisterMongoStores<TUser>(_TestingConnectionString);

			services.AddLogging();

			return services.BuildServiceProvider();
		}
	}
}