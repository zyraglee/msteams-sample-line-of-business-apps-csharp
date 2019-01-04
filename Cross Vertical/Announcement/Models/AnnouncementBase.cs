using AdaptiveCards;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CrossVertical.Announcement.Models
{
    public abstract class AnnouncementBase
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("tenantId")]
        public string TenantId { get; set; }

        [JsonProperty("ownerId")]
        public string OwnerId { get; set; }

        [JsonProperty("type")]
        public abstract string Type { get; set; }

        public DateTime CreatedTime { get; set; }

        public RecipientInfo Recipients { get; set; } = new RecipientInfo();
        public Schedule Schedule { get; set; }
        public Status Status { get; set; }

        bool IsAcknowledgementRequested { get; set; }
        bool IsContactAllowed { get; set; }
        MessageSensitivity Sensitivity { get; set; }

        public abstract AdaptiveCard GetCreateNewCard(List<Group> groups, List<Team> teams, bool isEditCard);
        public abstract AdaptiveCard GetPreviewCard();
    }
}