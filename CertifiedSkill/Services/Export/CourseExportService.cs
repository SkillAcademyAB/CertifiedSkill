using CertifiedSkill.Data;
using CertifiedSkill.Data.Participant;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace CertifiedSkill.Services.Export
{
    /// <summary>
    /// Exports course round results as CSV.
    /// PNR is never included in any export - by design.
    /// </summary>
    public class CourseExportService(ApplicationDbContext db)
    {
        /// <summary>
        /// Exports all certificates for a given course name as CSV.
        /// The export contains: DisplayName, Email, CourseName, CourseVersion, IssuedAt, Status.
        /// PNR is explicitly excluded.
        /// </summary>
        public async Task<string> ExportCertificatesCsvAsync(
            string courseName,
            CancellationToken cancellationToken = default)
        {
            var certificates = await db.Certificates
                .Include(c => c.Person)
                .Where(c => c.CourseName == courseName)
                .OrderBy(c => c.Person.DisplayName)
                .ToListAsync(cancellationToken);

            return BuildCsv(certificates);
        }

        /// <summary>
        /// Exports all certificates as CSV.
        /// PNR is explicitly excluded.
        /// </summary>
        public async Task<string> ExportAllCertificatesCsvAsync(
            CancellationToken cancellationToken = default)
        {
            var certificates = await db.Certificates
                .Include(c => c.Person)
                .OrderBy(c => c.CourseName)
                .ThenBy(c => c.Person.DisplayName)
                .ToListAsync(cancellationToken);

            return BuildCsv(certificates);
        }

        private static string BuildCsv(IEnumerable<Certificate> certificates)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DisplayName,Email,CourseName,CourseVersion,IssuedAt,Status");

            foreach (var cert in certificates)
            {
                var displayName = EscapeCsv(cert.Person.DisplayName);
                var email = EscapeCsv(cert.Person.Email);
                var courseName = EscapeCsv(cert.CourseName);
                var courseVersion = EscapeCsv(cert.CourseVersion);
                var issuedAt = cert.IssuedAt.ToString("yyyy-MM-dd");
                var status = cert.Status.ToString();

                sb.AppendLine($"{displayName},{email},{courseName},{courseVersion},{issuedAt},{status}");
            }

            return sb.ToString();
        }

        private static string EscapeCsv(string value)
        {
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }
    }
}
