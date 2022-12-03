using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NZWalks.API.Models.Domain;
using NZWalks.API.Models.DTO;
using NZWalks.API.Repositories;

namespace NZWalks.API.Controllers
{
    [ApiController]
    [Route("WalkDifficulty")]
    public class WalkDifficultyController : Controller
    {
        private readonly IMapper mapper;
        private readonly IWalkDifficultyRepository walkDifficultyRepository;

        public WalkDifficultyController(IMapper mapper, IWalkDifficultyRepository walkDifficultyRepository)
        {
            this.mapper = mapper;
            this.walkDifficultyRepository = walkDifficultyRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var walkDifficulties = await walkDifficultyRepository.GetAllAsync();
            var walkDifficultiesDto = mapper.Map<List<Models.DTO.WalkDifficulty>>(walkDifficulties);

            return Ok(walkDifficultiesDto);
        }

        [HttpGet]
        [Route("{id:guid}")]
        public async Task<IActionResult> GetAsync([FromRoute] Guid id)
        {
            var walkDifficulty = await walkDifficultyRepository.GetAsync(id);
            if (walkDifficulty == null)
            {
                return NotFound();
            }

            var walkDifficultyDto = mapper.Map<Models.DTO.WalkDifficulty>(walkDifficulty);

            return Ok(walkDifficultyDto);
        }

        [HttpPost]
        [ActionName("AddWalkDifficultyAsync")]
        public async Task<IActionResult> AddWalkDifficultyAsync([FromBody] Models.DTO.AddWalkDifficultyRequest addWalkDifficultyRequest)
        {
            // validate data now through fluent validator
            //if(!ValidAddWalkDifficultyAsyncData(addWalkDifficultyRequest))
            //{
            //    return BadRequest(ModelState);
            //}

            // convert addWalkDifficultyRequest to domain model
            var walkDifficultyDomainModel = new Models.Domain.WalkDifficulty()
            {
                Code = addWalkDifficultyRequest.Code
            };

            // create it using the repository and return it
            var response = await walkDifficultyRepository.CreateAsync(walkDifficultyDomainModel);

            // convert returned domain model to DTO and send status 
            var walkDifficultyDTO = mapper.Map<Models.DTO.WalkDifficulty>(response);

            return CreatedAtAction(nameof(AddWalkDifficultyAsync), new { id = walkDifficultyDTO.Id }, walkDifficultyDTO);
        }

        [HttpPut]
        [Route("{id:guid}")]
        public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] Models.DTO.UpdateWalkDifficultyRequest updateWalkDifficultyRequest)
        {
            // validate data
            if(!ValidUpdateAsyncData(updateWalkDifficultyRequest))
            {
                return BadRequest(ModelState);
            }

            var walkDifficultyDomainModel = new Models.Domain.WalkDifficulty()
            {
                Code = updateWalkDifficultyRequest.Code
            };

            walkDifficultyDomainModel = await walkDifficultyRepository.UpdateAsync(id, walkDifficultyDomainModel);
            if (walkDifficultyDomainModel == null)
            {
                return NotFound();
            }

            var walkDifficultyDTO = mapper.Map<Models.DTO.WalkDifficulty>(walkDifficultyDomainModel);

            return Ok(walkDifficultyDTO);
        }

        [HttpDelete]
        [Route("{id:guid}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
        {
            var deletedWalkDifficulty = await walkDifficultyRepository.DeleteAsync(id);
            if (deletedWalkDifficulty == null)
            {
                return NotFound();
            }

            var deletedWalkDifficultyDTO = mapper.Map<Models.DTO.WalkDifficulty>(deletedWalkDifficulty);

            return Ok(deletedWalkDifficultyDTO);
        }

        #region
        private bool ValidAddWalkDifficultyAsyncData(AddWalkDifficultyRequest addWalkDifficultyRequest)
        {
            if (addWalkDifficultyRequest == null)
            {
                ModelState.AddModelError(nameof(addWalkDifficultyRequest),
                    "addWalkDifficultyRequest cannot be null");

                return false;
            }

            if (string.IsNullOrWhiteSpace(addWalkDifficultyRequest.Code))
            {
                ModelState.AddModelError(nameof(addWalkDifficultyRequest.Code),
                    $"{nameof(addWalkDifficultyRequest.Code)} cannot be empty or white space.");
            }

            if (ModelState.ErrorCount > 0)
            {
                return false;
            }

            return true;
        }

        private bool ValidUpdateAsyncData(Models.DTO.UpdateWalkDifficultyRequest updateWalkDifficultyRequest)
        {
            if (updateWalkDifficultyRequest == null)
            {
                ModelState.AddModelError(nameof(updateWalkDifficultyRequest),
                    "addWalkDifficultyRequest cannot be null");

                return false;
            }

            if (string.IsNullOrWhiteSpace(updateWalkDifficultyRequest.Code))
            {
                ModelState.AddModelError(nameof(updateWalkDifficultyRequest.Code),
                    $"{nameof(updateWalkDifficultyRequest.Code)} cannot be empty or white space.");
            }

            if (ModelState.ErrorCount > 0)
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
