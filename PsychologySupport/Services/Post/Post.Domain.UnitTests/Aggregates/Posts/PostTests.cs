using Post.Domain.Aggregates.Posts.Enums;
using Post.Domain.Exceptions;

namespace Post.Tests.Domain.Unit.Aggregates.Posts
{
    public class PostTests
    {
        private readonly Guid _authorAliasId = Guid.NewGuid();
        private readonly Guid _authorAliasVersionId = Guid.NewGuid();
        private readonly string _content = "Test post content";
        private readonly string _title = "Test Post Title";

        [Fact]
        public void CreatePost_ShouldCreateValidPost()
        {
            // Act
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId,
                PostVisibility.Draft);

            // Assert
            Assert.NotEqual(Guid.Empty, post.Id);
            Assert.Equal(_authorAliasId, post.Author.AliasId);
            Assert.Equal(_authorAliasVersionId, post.Author.AliasVersionId);
            Assert.Equal(_content, post.Content.Value);
            Assert.Equal(_title, post.Content.Title);
            Assert.Equal(PostVisibility.Draft, post.Visibility);
            Assert.Equal(ModerationStatus.Pending, post.Moderation.Status);
            Assert.False(post.IsDeleted);
            Assert.False(post.IsCommentsLocked);
            Assert.False(post.IsEdited);
            Assert.False(post.HasMedia);
            Assert.False(post.HasCategories);
            Assert.False(post.IsPublished);
            Assert.Equal(0, post.Metrics.ViewCount);
            Assert.Equal(0, post.Metrics.CommentCount);
            Assert.Equal(0, post.Metrics.ReactionCount);
        }

        [Fact]
        public void UpdateContent_ShouldUpdatePostContent()
        {
            // Arrange
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId);
            
            var newContent = "Updated content";
            var newTitle = "Updated title";

            // Act
            post.UpdateContent(newContent, newTitle, _authorAliasId);

            // Assert
            Assert.Equal(newContent, post.Content.Value);
            Assert.Equal(newTitle, post.Content.Title);
            Assert.True(post.IsEdited);
            Assert.NotNull(post.EditedAt);
        }

        [Fact]
        public void UpdateContent_WithDifferentUser_ShouldThrowPostAuthorMismatchException()
        {
            // Arrange
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId);
            
            var differentUserId = Guid.NewGuid();

            // Act & Assert
            Assert.Throws<PostAuthorMismatchException>(() => 
                post.UpdateContent("New content", "New title", differentUserId));
        }

        [Fact]
        public void ChangeVisibility_ToPublicWithoutApproval_ShouldThrowInvalidPostDataException()
        {
            // Arrange
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId,
                PostVisibility.Draft);

            // Act & Assert
            Assert.Throws<InvalidPostDataException>(() => 
                post.ChangeVisibility(PostVisibility.Public, _authorAliasId));
        }

        [Fact]
        public void ChangeVisibility_SameVisibility_ShouldNotChangeAnything()
        {
            // Arrange
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId,
                PostVisibility.Draft);

            // Act
            post.ChangeVisibility(PostVisibility.Draft, _authorAliasId);

            // Assert
            Assert.Equal(PostVisibility.Draft, post.Visibility);
        }

        [Fact]
        public void AddMedia_ShouldAddMediaToPost()
        {
            // Arrange
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId);
            
            var mediaId = Guid.NewGuid();

            // Act
            post.AddMedia(mediaId);

            // Assert
            Assert.Single(post.Media);
            Assert.Equal(mediaId, post.Media[0].MediaId);
            Assert.True(post.HasMedia);
        }

        [Fact]
        public void AddMedia_DuplicateMedia_ShouldThrowInvalidPostDataException()
        {
            // Arrange
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId);
            
            var mediaId = Guid.NewGuid();
            post.AddMedia(mediaId);

            // Act & Assert
            Assert.Throws<InvalidPostDataException>(() => post.AddMedia(mediaId));
        }

        [Fact]
        public void AddMedia_ExceedingLimit_ShouldThrowInvalidPostDataException()
        {
            // Arrange
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId);

            // Add 10 media (maximum allowed)
            for (int i = 0; i < 10; i++)
            {
                post.AddMedia(Guid.NewGuid());
            }

            // Act & Assert
            Assert.Throws<InvalidPostDataException>(() => post.AddMedia(Guid.NewGuid()));
        }

        [Fact]
        public void RemoveMedia_ShouldRemoveMediaFromPost()
        {
            // Arrange
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId);
            
            var mediaId = Guid.NewGuid();
            post.AddMedia(mediaId);

            // Act
            post.RemoveMedia(mediaId, _authorAliasId);

            // Assert
            Assert.Empty(post.Media);
            Assert.False(post.HasMedia);
        }

        [Fact]
        public void RemoveMedia_NonExistentMedia_ShouldNotThrowException()
        {
            // Arrange
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId);
            
            var mediaId = Guid.NewGuid();
            var nonExistentMediaId = Guid.NewGuid();
            post.AddMedia(mediaId);

            // Act - Should not throw
            post.RemoveMedia(nonExistentMediaId, _authorAliasId);

            // Assert
            Assert.Single(post.Media);
            Assert.True(post.HasMedia);
        }

        [Fact]
        public void AddCategory_ShouldAddCategoryToPost()
        {
            // Arrange
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId);
            
            var categoryTagId = Guid.NewGuid();

            // Act
            post.AddCategory(categoryTagId);

            // Assert
            Assert.Single(post.Categories);
            Assert.Equal(categoryTagId, post.Categories[0].CategoryTagId);
            Assert.True(post.HasCategories);
        }

        [Fact]
        public void AddCategory_DuplicateCategory_ShouldNotAddDuplicate()
        {
            // Arrange
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId);
            
            var categoryTagId = Guid.NewGuid();
            post.AddCategory(categoryTagId);

            // Act
            post.AddCategory(categoryTagId); // Should be ignored, not throw

            // Assert
            Assert.Single(post.Categories);
            Assert.Equal(categoryTagId, post.Categories[0].CategoryTagId);
        }

        [Fact]
        public void AddCategory_ExceedingLimit_ShouldThrowInvalidPostDataException()
        {
            // Arrange
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId);

            // Add 5 categories (maximum allowed)
            for (int i = 0; i < 5; i++)
            {
                post.AddCategory(Guid.NewGuid());
            }

            // Act & Assert
            Assert.Throws<InvalidPostDataException>(() => post.AddCategory(Guid.NewGuid()));
        }

        [Fact]
        public void RemoveCategory_ShouldRemoveCategoryFromPost()
        {
            // Arrange
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId);
            
            var categoryTagId = Guid.NewGuid();
            post.AddCategory(categoryTagId);

            // Act
            post.RemoveCategory(categoryTagId, _authorAliasId);

            // Assert
            Assert.Empty(post.Categories);
            Assert.False(post.HasCategories);
        }

        [Fact]
        public void Approve_ShouldApprovePost()
        {
            // Arrange
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId);
            
            var moderatorId = Guid.NewGuid();
            var policyVersion = "1.0";

            // Act
            post.Approve(policyVersion, moderatorId);

            // Assert
            Assert.Equal(ModerationStatus.Approved, post.Moderation.Status);
            Assert.Equal(policyVersion, post.Moderation.PolicyVersion);
            Assert.True(post.CanBePublished);
        }

        [Fact]
        public void Reject_ShouldRejectPost()
        {
            // Arrange
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId);
            
            var moderatorId = Guid.NewGuid();
            var policyVersion = "1.0";
            var reasons = new List<string> { "Inappropriate content", "Violates guidelines" };

            // Act
            post.Reject(reasons, policyVersion, moderatorId);

            // Assert
            Assert.Equal(ModerationStatus.Rejected, post.Moderation.Status);
            Assert.Equal(policyVersion, post.Moderation.PolicyVersion);
            Assert.Equal(reasons, post.Moderation.Reasons);
            Assert.False(post.CanBePublished);
        }

        [Fact]
        public void ToggleCommentsLock_ShouldToggleCommentsLockStatus()
        {
            // Arrange
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId);
            
            // Act
            post.ToggleCommentsLock(_authorAliasId);

            // Assert
            Assert.True(post.IsCommentsLocked);

            // Toggle again
            post.ToggleCommentsLock(_authorAliasId);
            Assert.False(post.IsCommentsLocked);
        }

        [Fact]
        public void Delete_ShouldMarkPostAsDeleted()
        {
            // Arrange
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId);
            
            // Act
            post.Delete(_authorAliasId);

            // Assert
            Assert.True(post.IsDeleted);
            Assert.NotNull(post.DeletedAt);
            Assert.Equal(_authorAliasId.ToString(), post.DeletedByAliasId);
            Assert.False(post.CanBePublished);
        }

        [Fact]
        public void Restore_ByAuthor_ShouldRestoreDeletedPost()
        {
            // Arrange
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId);
            
            post.Delete(_authorAliasId);

            // Act
            post.Restore(_authorAliasId);

            // Assert
            Assert.False(post.IsDeleted);
            Assert.Null(post.DeletedAt);
            Assert.Null(post.DeletedByAliasId);
        }

        [Fact]
        public void Restore_ByDifferentUser_ShouldThrowPostAuthorMismatchException()
        {
            // Arrange
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId);
            
            post.Delete(_authorAliasId);
            var differentUserId = Guid.NewGuid();

            // Act & Assert
            Assert.Throws<PostAuthorMismatchException>(() => post.Restore(differentUserId));
        }

        [Fact]
        public void ActionOnDeletedPost_ShouldThrowDeletedPostActionException()
        {
            // Arrange
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId);
            
            post.Delete(_authorAliasId);

            // Act & Assert
            Assert.Throws<DeletedPostActionException>(() => 
                post.UpdateContent("New content", "New title", _authorAliasId));
            
            Assert.Throws<DeletedPostActionException>(() => 
                post.AddMedia(Guid.NewGuid()));
            
            Assert.Throws<DeletedPostActionException>(() => 
                post.ToggleCommentsLock(_authorAliasId));
        }

        [Fact]
        public void IncrementMetrics_ShouldUpdatePostMetrics()
        {
            // Arrange
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId);
            
            // Act
            post.IncrementReactionCount(2);
            post.IncrementCommentCount(3);
            post.RecordView();
            post.RecordView();

            // Assert
            Assert.Equal(2, post.Metrics.ReactionCount);
            Assert.Equal(3, post.Metrics.CommentCount);
            Assert.Equal(2, post.Metrics.ViewCount);
        }

        [Fact]
        public void DecrementMetrics_ShouldUpdatePostMetrics()
        {
            // Arrange
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId);
            
            post.IncrementReactionCount(5);
            post.IncrementCommentCount(4);

            // Act
            post.DecrementReactionCount(2);
            post.DecrementCommentCount(1);

            // Assert
            Assert.Equal(3, post.Metrics.ReactionCount);
            Assert.Equal(3, post.Metrics.CommentCount);
        }

        [Fact]
        public void SynchronizeCounters_ShouldSetMetricsDirectly()
        {
            // Arrange
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId);
            
            // Act
            post.SynchronizeCounters(10, 15);

            // Assert
            Assert.Equal(10, post.Metrics.ReactionCount);
            Assert.Equal(15, post.Metrics.CommentCount);
        }

        [Fact]
        public void SetCoverMedia_ShouldMarkMediaAsCover()
        {
            // Arrange
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId);
            
            var mediaId1 = Guid.NewGuid();
            var mediaId2 = Guid.NewGuid();
            post.AddMedia(mediaId1);
            post.AddMedia(mediaId2);

            // Act
            post.SetCoverMedia(mediaId2, _authorAliasId);

            // Assert
            var coverMedia = post.Media.FirstOrDefault(m => m.IsCover);
            Assert.NotNull(coverMedia);
            Assert.Equal(mediaId2, coverMedia.MediaId);
            Assert.False(post.Media.First(m => m.MediaId == mediaId1).IsCover);
        }

        [Fact]
        public void UpdateMediaAltText_ShouldUpdateAltText()
        {
            // Arrange
            var post = Post.Domain.Aggregates.Posts.Post.Create(
                _authorAliasId,
                _content,
                _title,
                _authorAliasVersionId);
            
            var mediaId = Guid.NewGuid();
            post.AddMedia(mediaId);
            var altText = "Alternative text description";

            // Act
            post.UpdateMediaAltText(mediaId, altText, _authorAliasId);

            // Assert
            var media = post.Media.First();
            Assert.Equal(altText, media.AltText);
        }
    }
}
