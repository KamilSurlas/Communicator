﻿using Communicator.Data;
using Communicator.Models;
using Communicator.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using System.Data.Entity;
using System.Drawing;

namespace Communicator.Hubs
{
    public class ChatHub : Hub
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ChatService _chatService;
        private readonly ApplicationDbContext _context;
        
        public ChatHub(UserManager<ApplicationUser> userManager, ChatService chatService, ApplicationDbContext applicationDbContext)
        {
            _userManager = userManager;
            _chatService = chatService;
            _context = applicationDbContext;
        }

        public override async Task OnConnectedAsync()
        {
            var user = await _userManager.GetUserAsync(Context.User);
            await Groups.AddToGroupAsync(Context.ConnectionId, "Communicator");
            _chatService.AddUser(user.Id, Context.ConnectionId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Communicator");
            await base.OnDisconnectedAsync(exception);
            var user = _chatService.GetUserByConnectionId(Context.ConnectionId);
            _chatService.RemoveUserFromList(user);

        }
        public async Task PreparePrivateGroup(string senderId, string receiverId)
        {
            string groupName = _chatService.GetPrivateGroupName(senderId, receiverId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);                
        }      
        public async Task SendMessage(string messageContent,string senderId, string receiverId)
        {           
            if (receiverId != null )
            {             
                var sender = await _userManager.FindByIdAsync(senderId);
                var senderName = $"{sender.Name} {sender.LastName}";
                string groupName = _chatService.GetPrivateGroupName(senderId, receiverId);
                await Clients.Group(groupName).SendAsync("ReceiveMessage", messageContent, DateTime.Now.ToString(), senderName);
               
            }
        }
    }
}
