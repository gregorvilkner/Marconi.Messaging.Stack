using MarconiRelay.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace MarconiRelay.Server.Controllers
{
    [ApiController]

    public class MarconiNrController : ControllerBase
    {
        private readonly MarconiKeyVaultClient _marconiKeyVaultClient;
        public MarconiNrController(IOptions<MarconiKeyVaultClient> MarconiKeyVaultClient)
        {
            _marconiKeyVaultClient = MarconiKeyVaultClient.Value;
        }

        [Authorize]
        [HttpGet]
        [Route("MarconiNr")]
        public async Task<IEnumerable<string>> GetActiveMarconiNumbers()
        {

            var UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            RelayHelper aRelayHelper = new RelayHelper(_marconiKeyVaultClient.ClientId, _marconiKeyVaultClient.ClientSecret, _marconiKeyVaultClient.TenantId);

            return await aRelayHelper.GetAllMarconiNrsAsync(UserId);

        }

        [HttpGet]
        [Route("MarconiNr/{MarconiNr}")]
        public async Task<string> ValidateMarconiNumber(string MarconiNr)
        {

            RelayHelper aRelayHelper = new RelayHelper(_marconiKeyVaultClient.ClientId, _marconiKeyVaultClient.ClientSecret, _marconiKeyVaultClient.TenantId);

            return await aRelayHelper.ValidateMarconiNr(MarconiNr);

        }


        [Authorize]
        [HttpPut]
        [Route("MarconiNr")]
        public async Task<string> Put()
        {
            var UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            RelayHelper aRelayHelper = new RelayHelper(_marconiKeyVaultClient.ClientId, _marconiKeyVaultClient.ClientSecret, _marconiKeyVaultClient.TenantId);

            return await aRelayHelper.CreateNewMarconiNr(UserId);

        }

        [Authorize]
        [HttpDelete]
        [Route("MarconiNr/{MarconiNr}")]
        public async Task Delete(string MarconiNr)
        {
            var UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            RelayHelper aRelayHelper = new RelayHelper(_marconiKeyVaultClient.ClientId, _marconiKeyVaultClient.ClientSecret, _marconiKeyVaultClient.TenantId);

            await aRelayHelper.DeleteMarconiNr(MarconiNr, UserId);

            return;

        }


    }
}
