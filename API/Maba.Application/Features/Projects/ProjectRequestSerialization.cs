using System.Text.Json;
using Maba.Application.Features.Projects.DTOs;
using Maba.Domain.Projects;

namespace Maba.Application.Features.Projects;

public static class ProjectRequestSerialization
{
    public static string? SerializeCapabilities(List<string>? values)
    {
        if (values == null)
        {
            return null;
        }

        var normalized = values
            .Where(static x => !string.IsNullOrWhiteSpace(x))
            .Select(static x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return normalized.Count == 0 ? null : JsonSerializer.Serialize(normalized);
    }

    public static List<string> DeserializeCapabilities(ProjectRequest request) =>
        DeserializeCapabilities(request.RequiredCapabilitiesJson);

    public static List<string> DeserializeCapabilities(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public static string? SerializeAttachments(List<ProjectRequestAttachmentDto>? attachments, string? fallbackUrl, string? fallbackFileName)
    {
        var normalized = (attachments ?? new List<ProjectRequestAttachmentDto>())
            .Where(static x => !string.IsNullOrWhiteSpace(x.Url) && !string.IsNullOrWhiteSpace(x.FileName))
            .Select(static x => new ProjectRequestAttachmentDto
            {
                Url = x.Url.Trim(),
                FileName = x.FileName.Trim()
            })
            .ToList();

        if (normalized.Count == 0 && !string.IsNullOrWhiteSpace(fallbackUrl) && !string.IsNullOrWhiteSpace(fallbackFileName))
        {
            normalized.Add(new ProjectRequestAttachmentDto
            {
                Url = fallbackUrl.Trim(),
                FileName = fallbackFileName.Trim()
            });
        }

        return normalized.Count == 0 ? null : JsonSerializer.Serialize(normalized);
    }

    public static List<ProjectRequestAttachmentDto> DeserializeAttachments(ProjectRequest request) =>
        DeserializeAttachments(request.AttachmentsJson, request.AttachmentUrl, request.AttachmentFileName);

    public static List<ProjectRequestAttachmentDto> DeserializeAttachments(string? json, string? fallbackUrl, string? fallbackFileName)
    {
        if (!string.IsNullOrWhiteSpace(json))
        {
            try
            {
                var deserialized = JsonSerializer.Deserialize<List<ProjectRequestAttachmentDto>>(json);
                if (deserialized != null && deserialized.Count > 0)
                {
                    return deserialized;
                }
            }
            catch
            {
            }
        }

        if (!string.IsNullOrWhiteSpace(fallbackUrl) && !string.IsNullOrWhiteSpace(fallbackFileName))
        {
            return new List<ProjectRequestAttachmentDto>
            {
                new()
                {
                    Url = fallbackUrl,
                    FileName = fallbackFileName
                }
            };
        }

        return new List<ProjectRequestAttachmentDto>();
    }
}
