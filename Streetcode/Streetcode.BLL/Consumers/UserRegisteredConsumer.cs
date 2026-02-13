using MassTransit;
using Streetcode.DAL.Entities.Users;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Shared.DTO.Events;
using Streetcode.Shared.Enums;

namespace Streetcode.BLL.Consumers
{
    public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public UserRegisteredConsumer(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
        {
            var message = context.Message;

            var existingUser = await _repositoryWrapper.UserRepository
                .GetFirstOrDefaultAsync(u => u.Id == message.UserId || u.Email == message.Email);

            if (existingUser != null)
            {
                return;
            }

            var newUser = new User
            {
                Id = message.UserId,
                Name = message.Name,
                Surname = message.Surname,
                Email = message.Email,
                PhoneNumber = message.PhoneNumber,
                Role = message.Role
            };

            await _repositoryWrapper.UserRepository.CreateAsync(newUser);
            await _repositoryWrapper.SaveChangesAsync();
        }
    }
}
