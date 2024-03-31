using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SlackBot.Handlers;
using SlackBot.Models;
using SlackNet;
using SlackNet.AspNetCore;

namespace SlackBot.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CheckPresenceController : ControllerBase
    {
        private readonly TodoContext _todoContext;
        private readonly ISlackRequestHandler _slackRequestHandler;
        private readonly ISlackApiClient _slackApiClient;
        private readonly SlackEndpointConfiguration _slackEndpointConfiguration;
        private readonly EchoDemo _echoDemo;


        public CheckPresenceController(TodoContext todoContext, ISlackRequestHandler requestHandler, SlackEndpointConfiguration slackEndpointConfiguration, ISlackApiClient slackApiClient, EchoDemo echoDemo)
        {
            _todoContext = todoContext;
            _slackRequestHandler = requestHandler;
            _slackEndpointConfiguration = slackEndpointConfiguration;
            _slackApiClient = slackApiClient;
            _echoDemo = echoDemo;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _todoContext.Members.FindAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost("createmember")]
        public async Task<IActionResult> PostMember(Member member)
        {
            if (member == null) { return BadRequest(); }
            _todoContext.Members.Add(member);
            await _todoContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserById), new { id = member.Id }, member);

        }

        [HttpPost("submitform")]
        public async Task<ActionResult> Submit([FromBody] Member request)
        {
            if ((DateTime.UtcNow - _echoDemo.formSentTime).TotalMinutes >= _echoDemo.deadline)
            {
                return Content("form is locked", "text/plain");
            }
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Name and gmail fields is required.");
            }
            var member = await _todoContext.Members
                .FirstOrDefaultAsync(e => e.Name == request.Name);
            if (member == null) { return NotFound(); }
            AttendenceRecord attendenceRecord = new AttendenceRecord()
            {
                MemberId = member.Id,
                Date = DateTime.UtcNow,
                IsPresent = true,
                Member = member

            };
            _todoContext.AttendenceRecords.Add(attendenceRecord);
            await _todoContext.SaveChangesAsync();
            return Ok();
        }
    }
}
