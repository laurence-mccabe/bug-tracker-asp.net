﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheBugTracker.Data;
using TheBugTracker.Models; 
using TheBugTracker.Models.Enums;
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IBTRolesService _rolesService;
        private readonly IBTProjectService _projectService;

        public BTTicketService(ApplicationDbContext context,
                               RoleManager<IdentityRole> roleManager,
                               IBTRolesService rolesService,
                               IBTProjectService projectService)
        {
            _context = context;
            _roleManager = roleManager;
            _rolesService = rolesService;
            _projectService = projectService;
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task AddNewTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }

        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task ArchiveTicketAsync(Ticket ticket)
        {
            try
            {
                ticket.Archived = true;
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }

        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task AddTicketCommentAsync(TicketComment ticketComment)
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        {
            try
            {
                await _context.AddAsync(ticketComment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment)
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        {
            try
            {
                await _context.AddAsync(ticketAttachment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task AssignTicketAsync(int ticketId, string userId)
        {
           
            Ticket ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            try
            {
                if (ticket != null)
                {
                    try
                    {
                        ticket.DeveloperUserId = userId;
                        ticket.TicketStatusId = (await LookupTicketStatusIdAsync("Development")).Value;
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task<List<Ticket>> GetAllTicketsForAdmin()
        {
            List<Ticket> tickets = await _context.Tickets
                                                 .Include(t => t.Project)
                                                 .Include(t => t.TicketType)
                                                 .Include(t => t.TicketPriority)
                                                 .Include(t => t.TicketStatus)
                                                 .Include(t => t.OwnerUser)
                                                 .Include(t => t.DeveloperUser)
                                                 .Include(t => t.Comments)
                                                    .ThenInclude(t=>t.User)
                                                 .Include(t => t.Comments)
                                                    .ThenInclude(t => t.Ticket)
                                                 .Include(t => t.Attachments)
                                                    .ThenInclude(t => t.User)
                                                 .Include(t => t.Attachments)
                                                    .ThenInclude(t => t.Ticket)
                                                 .Include(t => t.Notifications)
                                                    .ThenInclude(t => t.Ticket)
                                                 .Include(t => t.Notifications)
                                                    .ThenInclude(t => t.Recipient)
                                                 .Include(t => t.Notifications)
                                                    .ThenInclude(t => t.Sender)
                                                 .Include(t => t.History)
                                                    .ThenInclude(t=>t.Ticket)
                                                 .Include(t => t.History)
                                                    .ThenInclude(t => t.User)
                                                 .ToListAsync();
            return tickets;
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task<List<Ticket>> GetAllTicketsByCompanyAsync(int companyId)
        {
          
            try
            {
                List<Ticket> tickets = await _context.Projects
                                                     .Where(p => p.CompanyId == companyId)
                                                     .SelectMany(p => p.Tickets)
                                                         .Include(t => t.Attachments)
                                                         .Include(t => t.Comments)
                                                         .Include(t => t.History)
                                                         .Include(t => t.DeveloperUser)
                                                         .Include(t => t.OwnerUser)
                                                         .Include(t => t.TicketPriority)
                                                         .Include(t => t.TicketStatus)
                                                         .Include(t => t.TicketType)
                                                         .Include(t => t.Project)
                                                    .ToListAsync();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }

           
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task<List<Ticket>> GetAllTicketsByPriorityAsync(int companyId, string priorityName)
        {
            
            int priorityId = (await LookupTicketPriorityIdAsync(priorityName)).Value;

            try
            {
                List<Ticket> tickets = await _context.Projects
                                                      .Where(p=>p.CompanyId == companyId)
                                                      .SelectMany(p=>p.Tickets)
                                                         .Include(t => t.Attachments)
                                                         .Include(t => t.Comments)
                                                         .Include(t => t.History)
                                                         .Include(t => t.DeveloperUser)
                                                         .Include(t => t.OwnerUser)
                                                         .Include(t => t.TicketPriority)
                                                         .Include(t => t.TicketStatus)
                                                         .Include(t => t.TicketType)
                                                         .Include(t => t.Project)
                                                      .Where(t => t.TicketPriorityId == priorityId)
                                                         .ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }

            var ticks = await _context.Projects.Where(p => p.CompanyId == companyId).Include(p => p.Tickets).ToListAsync();
            var ticks2 = ticks.SelectMany(t => t.Tickets).Where(t=>t.TicketPriority.Name == priorityName).ToList();
            return ticks2;

        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task<List<Ticket>> GetAllTicketsByStatusAsync(int companyId, string statusName)
        {
            int statusId = (await LookupTicketStatusIdAsync(statusName)).Value;

            try
            {
                List<Ticket> tickets = await _context.Projects
                                                      .Where(p => p.CompanyId == companyId)
                                                     .SelectMany(p => p.Tickets)
                                                         .Include(t => t.Attachments)
                                                         .Include(t => t.Comments)
                                                         .Include(t => t.History)
                                                         .Include(t => t.DeveloperUser)
                                                         .Include(t => t.OwnerUser)
                                                         .Include(t => t.TicketPriority)
                                                         .Include(t => t.TicketStatus)
                                                         .Include(t => t.TicketType)
                                                         .Include(t => t.Project)
                                                     .Where(t => t.TicketStatusId == statusId)
                                                  
                                                         .ToListAsync();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
            
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task<List<Ticket>> GetAllTicketsByTypeAsync(int companyId, string typeName)
        {
            int typeId = (await LookupTicketTypeIdAsync(typeName)).Value;

            try
            {
                List<Ticket> tickets = await _context.Projects
                                                     .Where(p => p.CompanyId == companyId)
                                                     .SelectMany(p => p.Tickets)
                                                         .Include(t => t.Attachments)
                                                         .Include(t => t.Comments)
                                                         .Include(t => t.History)
                                                         .Include(t => t.DeveloperUser)
                                                         .Include(t => t.OwnerUser)
                                                         .Include(t => t.TicketPriority)
                                                         .Include(t => t.TicketStatus)
                                                         .Include(t => t.TicketType)
                                                         .Include(t => t.Project)
                                                      .Where(t => t.TicketTypeId == typeId)
                                                     
                                                         .ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
            //how i did it:
            var ticks = await _context.Projects.Where(p => p.CompanyId == companyId).Include(t => t.Tickets).SelectMany(t => t.Tickets).ToListAsync();
            var ticks2 = ticks.Where(t => t.TicketType.Name == typeName).ToList();
            return ticks2;
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public async  Task<List<Ticket>> GetArchivedTicketsAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = (await GetAllTicketsByCompanyAsync(companyId)).Where(t => t.Archived == true).ToList();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task<List<Ticket>> GetProjectTicketsByPriorityAsync(string priorityName, int companyId, int projectId)
        {
            //how he did it:
            try
            {
                var t = (await GetAllTicketsByPriorityAsync(companyId,priorityName)).Where(t => t.ProjectId == projectId).ToList();
                return t;
            }
            catch (Exception)
            {

                throw;
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task<List<Ticket>> GetProjectTicketsByRoleAsync(string role, string userId, int projectId, int companyId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetTicketsByRoleAsync(role, userId, companyId)).Where(p => p.Id == projectId).ToList();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }

        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task<List<Ticket>> GetProjectTicketsByStatusAsync(string statusName, int companyId, int projectId)
        {
            try
            {
                var t = (await GetAllTicketsByStatusAsync(companyId, statusName)).Where(t => t.ProjectId == projectId).ToList();
                return t;
            }
            catch (Exception)
            {

                throw;
            }
            
            
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task<List<Ticket>> GetProjectTicketsByTypeAsync(string typeName, int companyId, int projectId)
        {
            try
            {
                var t = (await GetAllTicketsByTypeAsync(companyId, typeName)).Where(t => t.ProjectId == projectId).ToList();
                return t;
            }
            catch (Exception)
            {

                throw;
            }
            
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task<List<Ticket>> GetUnassignedTicketsAsync(int companyId)
        {
            //this method is to get the tickets which do not have a developer assigned to it.
            List<Ticket> tickets = new();

            try
            {   
                tickets = (await GetAllTicketsByCompanyAsync(companyId)).Where(t => string.IsNullOrEmpty(t.DeveloperUserId)).ToList();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId)
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        {
            try
            {
                TicketAttachment ticketAttachment = await _context.TicketAttachments
                                                                  .Include(t => t.User)
                                                                  .FirstOrDefaultAsync(t => t.Id == ticketAttachmentId);
                return ticketAttachment;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<Ticket> GetTicketAsNoTrackingAsync(int ticketId)
        {
            try
            {
                var ticks = await _context.Tickets    .Include(t => t.DeveloperUser)
                                                      .Include(t => t.Project)
                                                      .Include(t => t.TicketPriority)
                                                      .Include(t => t.TicketStatus)
                                                      .Include(t => t.TicketType)
                                                       .AsNoTracking()
                                                  .Where(t => t.Id == ticketId).FirstOrDefaultAsync();
                return ticks;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            try
            {
                var ticks = await _context.Tickets.Include(t => t.DeveloperUser)
                                                      .Include(t => t.OwnerUser)
                                                      .Include(t => t.Project)
                                                      .Include(t => t.TicketPriority)
                                                      .Include(t => t.TicketStatus)
                                                      .Include(t => t.TicketType)
                                                      .Include(t => t.Comments)
                                                      .Include(t => t.Attachments)
                                                      .Include(t=>  t.History)
                                                  .Where(t => t.Id == ticketId).FirstOrDefaultAsync();
                return ticks;
            }
            catch (Exception)
            {

                throw;
            }
            
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task<BTUser> GetTicketDeveloperAsync(int ticketId, int companyId)
        {
            BTUser developer = new();

            try
            {
                Ticket ticket = (await GetAllTicketsByCompanyAsync(companyId)).FirstOrDefault(t=>t.Id == ticketId);
                if(ticket?.DeveloperUserId != null)
                {
                    developer = ticket.DeveloperUser;
                }
                return developer;
            }
            catch (Exception)
            {

                throw;
            }
            
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task<List<Ticket>> GetTicketsByRoleAsync(string role, string userId, int companyId)
        {
            List<Ticket> tickets = new();
            try
            {
                if(role == Roles.Admin.ToString())
                {
                    tickets = await GetAllTicketsByCompanyAsync(companyId);
                }
                else if(role == Roles.Developer.ToString())
                { 
                    tickets = (await GetAllTicketsByCompanyAsync(companyId)).Where(t => t.DeveloperUserId == userId).ToList(); 
                }
                else if (role == Roles.Submitter.ToString())
                {
                    tickets = (await GetAllTicketsByCompanyAsync(companyId)).Where(t => t.OwnerUserId == userId).ToList();
                }
                else if (role == Roles.ProjectManager.ToString())
                {
                    tickets = (await GetTicketsByUserIdAsync(userId,companyId));
                }
                return tickets;

            }
            catch (Exception)
            {

                throw;
            }
           
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId)
        {
            BTUser btuser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            List<Ticket> tickets = new();

            try
                
            {
                if (await _rolesService.IsUserInRoleAsync(btuser, Roles.Admin.ToString()))
                {
                    tickets = (await _projectService.GetAllProjectsByCompany(companyId)).SelectMany(p => p.Tickets).ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(btuser, Roles.Developer.ToString()))
                {
                    tickets = (await _projectService.GetAllProjectsByCompany(companyId))
                                                    .SelectMany(p => p.Tickets).Where(t=>t.DeveloperUserId == userId).ToList();
                }
                else if ((await _rolesService.IsUserInRoleAsync(btuser, Roles.Submitter.ToString())))
                {
                    tickets = (await _projectService.GetAllProjectsByCompany(companyId))
                                                    .SelectMany(p => p.Tickets).Where(t => t.OwnerUserId == userId).ToList();
                }
                else if ((await _rolesService.IsUserInRoleAsync(btuser, Roles.ProjectManager.ToString())))
                {
                    tickets = (await _projectService.GetUserProjectsAsync(userId)).SelectMany(t => t.Tickets).ToList();
                }
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task<int?> LookupTicketPriorityIdAsync(string priorityName)
        {
            try
            {
                TicketPriority priority = await _context.TicketPriorities.FirstOrDefaultAsync(t=> t.Name == priorityName);
                return priority?.Id;
            }
            catch
            {
                throw;
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task<int?> LookupTicketStatusIdAsync(string statusName)
        {
            try
            {
                TicketStatus status = await _context.TicketStatuses.FirstOrDefaultAsync(t => t.Name == statusName);
                return status?.Id;
            }
            catch
            {
                throw;
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task<int?> LookupTicketTypeIdAsync(string typeName)
        {
            try
            {
                TicketType type = await _context.TicketTypes.FirstOrDefaultAsync(t => t.Name == typeName);
                return type?.Id;
            }
            catch
            {
                throw;
            }

        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task UpdateTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
            

        }

    }
}
