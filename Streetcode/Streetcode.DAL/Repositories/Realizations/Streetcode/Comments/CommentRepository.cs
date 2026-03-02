using Streetcode.DAL.Entities.Streetcode.Comments;
using Streetcode.DAL.Persistence;
using Streetcode.DAL.Repositories.Interfaces.Streetcode.Comments;
using Streetcode.DAL.Repositories.Realizations.Base;

namespace Streetcode.DAL.Repositories.Realizations.Streetcode.Comments
{
    public class CommentRepository : RepositoryBase<Comment>, ICommentRepository
    {
        public CommentRepository(StreetcodeDbContext streetcodeDbContext)
            : base(streetcodeDbContext)
        {
        }
    }
}
