using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using Sport.Service.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;

namespace Sport.Service.Controllers
{
	[AuthorizeLevel(AuthorizationLevel.User)]
	public class GameResultController : TableController<GameResult>
	{
		AppContext _context = new AppContext();

		protected override void Initialize(HttpControllerContext controllerContext)
		{
			base.Initialize(controllerContext);
			DomainManager = new EntityDomainManager<GameResult>(_context, Request, Services);
		}

		IQueryable<GameResultDto> ConvertGameResultToDto(IQueryable<GameResult> queryable)
		{
			return queryable.Select(dto => new GameResultDto
			{
				Id = dto.Id,
				DateCreated = dto.CreatedAt,
				ChallengeId = dto.ChallengeId,
				UpdatedAt = dto.UpdatedAt,
				ChallengeeScore = dto.ChallengeeScore,
				ChallengerScore = dto.ChallengerScore,
				Index = dto.Index
			});
		}

		// GET tables/GameResult
		public IQueryable<GameResultDto> GetAllGameResults()
		{
			return ConvertGameResultToDto(Query());
		}

		// GET tables/GameResult/48D68C86-6EA6-4C25-AA33-223FC9A27959
		public SingleResult<GameResultDto> GetGameResult(string id)
		{
			return SingleResult<GameResultDto>.Create(ConvertGameResultToDto(Lookup(id).Queryable));
		}

		// PATCH tables/GameResult/48D68C86-6EA6-4C25-AA33-223FC9A27959
		//public Task<GameResult> PatchGameResult(string id, Delta<GameResult> patch)
		//{
		//	return UpdateAsync(id, patch);
		//}

		// POST tables/GameResult
		public async Task<IHttpActionResult> PostGameResult(GameResultDto item)
		{
			GameResult current = await InsertAsync(item.ToGameResult());
			var result = CreatedAtRoute("Tables", new { id = current.Id }, current);
			return result;
		}

		// DELETE tables/GameResult/48D68C86-6EA6-4C25-AA33-223FC9A27959
		//public Task DeleteGameResult(string id)
		//{
		//	var task = DeleteAsync(id);
		//	return task;
		//}
	}
}