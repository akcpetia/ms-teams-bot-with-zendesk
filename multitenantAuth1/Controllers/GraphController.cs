using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using Microsoft.Graph;
using multitenantAuth1.Model;
using multitenantAuth1.Enums;
using multitenantAuth1.IServices;
using multitenantAuth1.Services;

namespace multitenantAuth1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
   
    public class GraphController : ControllerBase
    {
      
        private readonly IGraphService graphService;

        private readonly ILogger<GraphController> _logger;

        public GraphController(ILogger<GraphController> logger,  IGraphService _graphService)
        {
            _logger = logger; 
            graphService = _graphService;
        }



        [HttpGet("GetUserslist")]
        public async Task<IActionResult> GetMeinfo()
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                var locatioList = await graphService.GetUserData();
                responseModel.StatusCode = 200;
                responseModel.Message = MessagesEnum.Success.ToString();
                responseModel.Data = locatioList;
                return Ok(responseModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong in the {nameof(GetMeinfo)} action {ex}");
                responseModel.StatusCode = 500;
                responseModel.Message = MessagesEnum.Failed.ToString();
                responseModel.Data = "Internal server error";
                return Ok(responseModel);
            }
        }

        [HttpGet("CreateSubscription")]
        public async Task<IActionResult> CreateSubscription()
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                var locatioList = await graphService.CreateSubscription();
                responseModel.StatusCode = 200;
                responseModel.Message = MessagesEnum.Success.ToString();
                responseModel.Data = locatioList;
                return Ok(responseModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong in the {nameof(GetMeinfo)} action {ex}");
                responseModel.StatusCode = 500;
                responseModel.Message = MessagesEnum.Failed.ToString();
                responseModel.Data = "Internal server error";
                return Ok(responseModel);
            }
        }

        [HttpGet("GetAllSubscription")]
        public async Task<IActionResult> GetAllSubscription() 
        {

            ResponseModel responseModel = new ResponseModel();
            try
            {

                dynamic data = await graphService.GetAllSubscription();
                responseModel.StatusCode = 200;
                responseModel.Message = "Success";
                responseModel.Data = data;
                return Ok(responseModel);
            }
            catch (Exception ex)
            {

                responseModel.StatusCode = 500;
                responseModel.Message = "Something went wrong";
                responseModel.Data = "Internal server error " + ex;
                return Ok(responseModel);
            }

        }

        [HttpGet("DeleteSubscription")]
        public async Task<IActionResult> DeleteSubscription(string id)
        {

            ResponseModel responseModel = new ResponseModel();
            try
            {

                dynamic data = await graphService.DeleteSubscription(id);
                responseModel.StatusCode = 200;
                responseModel.Message = "Success";
                responseModel.Data = data;
                return Ok(responseModel);
            }
            catch (Exception ex)
            {

                responseModel.StatusCode = 500;
                responseModel.Message = "Something went wrong";
                responseModel.Data = "Internal server error " + ex;
                return Ok(responseModel);
            }

        }



    }
}