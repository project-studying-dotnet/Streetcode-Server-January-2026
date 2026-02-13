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

            var newUser = new User
            {
                Id = message.UserId,
                Name = message.Name,
                Surname = message.Surname,
                Email = message.Email,
                PhoneNumber = message.PhoneNumber,
                Role = UserRole.User
            };

            await _repositoryWrapper.UserRepository.CreateAsync(newUser);
            await _repositoryWrapper.SaveChangesAsync();
        }
    }
}
