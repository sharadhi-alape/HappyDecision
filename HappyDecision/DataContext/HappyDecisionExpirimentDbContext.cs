using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using HappyDecision.Models;

namespace HappyDecision.DataContext
{
    public class HappyDecisionExpirimentDbContext : DbContext
    {
        public HappyDecisionExpirimentDbContext()
            : base("name = DefaultConnection") { }

        public DbSet<Matrix> Matrices { get; set; }
        public DbSet<MatrixPositions> MatrixPositions { get; set; }
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<VotingDistribution> VotingDistributions { get; set; }
        public DbSet<ParameterValue> ParameterValues { get; set; }
        public DbSet<Parameter> Parameters { get; set; }

        public DbSet<SurveyResult> SurveyResults { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Demography> Demographies { get; set; }
        public DbSet<Consent> Consents { get; set; }
        public DbSet<Error> Errors { get; set; }
        public DbSet<ReadingTime> ReadingTimes { get; set; }

    }
}