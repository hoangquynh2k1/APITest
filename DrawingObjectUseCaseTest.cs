using AutoMapper;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Rvsta.SptApps.Biz02.Application.Constants;
using Rvsta.SptApps.Biz02.Application.Models;
using Rvsta.SptApps.Biz02.Application.Models.Api.V1.YieldDrawingsController;
using Rvsta.SptApps.Biz02.Application.Models.BatchUpdate;
using Rvsta.SptApps.Biz02.Application.UseCase;
using Rvsta.SptApps.Biz02.Infrastructure.Data.DataContext;
using Rvsta.SptApps.Biz02.Infrastructure.Data.Repositories;
using Rvsta.SptApps.Biz02.TestScenario.Tests.Data.DataContext;
using Rvsta.SptCore.Application.Services;
using Rvsta.SptCore.UnitTest.Utilities;
using static Rvsta.SptApps.Biz02.Application.UseCase.DrawingObjectUseCase;

namespace Rvsta.SptApps.Biz02.Application.Tests.UseCase
{
    [TestFixture(TestOf = typeof(DrawingObjectUseCase))]
    public class DrawingObjectUseCaseTest
    {
        private TestAppDataContext _dbContext = null!;
        private SqliteConnection _connection = null!;
        private static readonly DateTime NowDateTime = new(2023, 07, 31);
        private static readonly DateOnly TargetMonth = new(2023, 07, 01);

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Database.CloseConnectionAsync();
            await _dbContext.DisposeAsync();
            _connection.Dispose();
        }

        [OneTimeSetUp]
        public async Task OneTimeSetUpAsync()
        {
            var toDay = new DateTimeOffset(NowDateTime);

            var userContext = new UserContext
            {
                UserId = 2,
                LoginId = "unitTestUser",
                MyCompanyId = 2,
                AccessibleFieldIds = new[] { 2L },
                AccessibleCompanyOfficeIds = new[] { 2L }
            };

            var fieldWorkType = new Data.Entities.FieldWorkType
            {
                Id = 2,
                FieldId = 2,
                CreateUserId = "2",
                CreateProgramId = "2",
                ProgressIndexId = 2,
                FieldUniqueProgressIndex = "index",
                MainWorkTypeId = 2,
                SubWorkTypeId = 2,
                FieldUniqueSubWorkTypeName = "SubWork"
            };
            var yieldManagementTarget = new Data.Entities.YieldManagementTarget
            {
                Id = 1,
                ScheduleQuantity = 60,
                CreateUserId = "2",
                CreateProgramId = "2",
                FieldWorkType = fieldWorkType,
                FieldWorkTypeId = 2,
            };
            var yieldResult = new Data.Entities.YieldResult
            {
                Id = 1,
                YieldManagementTarget = yieldManagementTarget,
                FieldTreeId = 111,
                CreateUserId = "1",
                CreateProgramId = "1",
                InputType = InputType.Number,
                ConfirmStatus = ConfirmStatus.Confirm,
                ReportDay = new DateOnly(2023, 07, 01),
                ReportTargetMonth = new DateOnly(2023, 07, 01),
                ResultQuantity = 50
            };
            var yieldDrawing = new Data.Entities.YieldDrawing
            {
                Id = 2,
                CreateUserId = "2",
                YieldManagementTarget = yieldManagementTarget,
                FieldTreeId = 222,
                BackgroundPath = "background.png",
                CreateProgramId = "2",
            };
            var drawingObjectConstruction = new Data.Entities.DrawingObject
            {
                Id = 1,
                CreateProgramId = "1",
                YieldDrawing = yieldDrawing,
                DrawingType = DrawingType.ConstructionScope,
                DrawingData = "Construction",
                TargetMonth = new DateOnly(2023, 7, 1),
                CreateUserId = "1",
            };
            var drawingObjectClaim = new Data.Entities.DrawingObject
            {
                Id = 2,
                CreateProgramId = "1",
                YieldDrawing = yieldDrawing,
                DrawingType = DrawingType.ClaimScope,
                DrawingData = "Claim",
                TargetMonth = new DateOnly(2023, 7, 1),
                CreateUserId = "1"
            };

            var testContext = new TestContextHelper().CreateTestContext(
                "unitTest",
                new DateTimeOffset(toDay.DateTime),
                userContext
            );
            _connection = new SqliteConnection(TestDbContextFactory.GetDefaultConnectionStringBuilder().ToString());
            _dbContext =
                new TestAppDataContext(
                    TestDbContextFactory.CreateDataContextOptions<AppDataContext>(_connection, testContext.Interceptor)
                );
            await _dbContext.Database.OpenConnectionAsync();
            await _dbContext.Database.EnsureCreatedAsync();

            await _dbContext.FieldWorkTypes.AddAsync(fieldWorkType);
            await _dbContext.YieldManagementTargets.AddAsync(yieldManagementTarget);
            await _dbContext.YieldResults.AddAsync(yieldResult);
            await _dbContext.YieldDrawings.AddAsync(yieldDrawing);
            await _dbContext.DrawingObjects.AddRangeAsync(drawingObjectConstruction, drawingObjectClaim);
            await _dbContext.SaveChangesAsync();
        }

        public static IEnumerable<TestCaseData> UpdateBatchObjectAsyncTestCases
        {
            get
            {
                yield return new TestCaseData(
                    new BatchUpdateYieldDrawingRequest(
                        RowState.Edited,
                        2,
                        1,
                        1,
                        "background.jpg",
                        Convert.ToBase64String(new byte[] { 0 }),
                        new List<UpdateDrawingRequest>
                        {
                            new(
                                RowState.Edited,
                                new DrawingObject
                                {
                                    Id = 1,
                                    YieldDrawingId = 2,
                                    DrawingType = 0,
                                    DrawingData = "DrawingData1",
                                    TargetMonth = TargetMonth,
                                    RowVersion = Convert.ToBase64String(new byte[] { 0 })
                                })
                        }),
                    new BatchUpdateResults<DrawingObjectResponse>
                    {
                        new()
                        {
                            RowState = RowState.Edited,
                            DbUpdateStatus = DbUpdateStatus.Success,
                            Data = new DrawingObjectResponse
                            {
                                Id = 1,
                                DrawingType = 0,
                                DrawingData = "DrawingData1",
                                TargetMonth = "2023-07",
                                RowVersion = Convert.ToBase64String(new byte[] { 0 })
                            }
                        }
                    },
                    string.Empty)
                    .SetName("更新する描画オブジェクト情報が正しく場合、データを更新すると結果を返す");

                yield return new TestCaseData(
                    new BatchUpdateYieldDrawingRequest(
                        RowState.Added,
                        0,
                        1,
                        1,
                        "background.jpg",
                        Convert.ToBase64String(new byte[] { 0 }),
                        new List<UpdateDrawingRequest>
                        {
                            new(
                                RowState.Added,
                                new DrawingObject
                                {
                                    DrawingType = 0,
                                    DrawingData = "data 1",
                                    TargetMonth = new DateOnly(2023, 7, 1),
                                    RowVersion = Convert.ToBase64String(new byte[] { 0 })
                                })
                        }),
                    new BatchUpdateResults<DrawingObjectResponse>
                    {
                        new()
                        {
                            RowState = RowState.Added,
                            DbUpdateStatus = DbUpdateStatus.Success,
                            Data = new DrawingObjectResponse
                            {
                                Id = 1,
                                DrawingType = 0,
                                DrawingData = "data 1",
                                TargetMonth = "2023-07",
                                RowVersion = Convert.ToBase64String(new byte[] { 0 })
                            }
                        }
                    },
                    null)
                    .SetName("出来高図面が存在しない場合、出来高図面や描画オブジェクトを新規追加する");

                yield return new TestCaseData(
                    new BatchUpdateYieldDrawingRequest(
                        RowState.Edited,
                        2,
                        1,
                        1,
                        "background.jpg",
                        Convert.ToBase64String(new byte[] { 0 }), new List<UpdateDrawingRequest>
                        {
                            new(
                                RowState.Edited,
                                new DrawingObject
                                {
                                    Id = 6,
                                    YieldDrawingId = 2,
                                    DrawingType = 0,
                                    DrawingData = "DrawingData1",
                                    TargetMonth = TargetMonth,
                                    RowVersion = Convert.ToBase64String(new byte[] { 0 })
                                })
                        }),
                    new BatchUpdateResults<DrawingObjectResponse>(),
                    "record not found")
                    .SetName("指摘した描画オブジェクトが存在しない場合、エラーを返す。");

                yield return new TestCaseData(
                    new BatchUpdateYieldDrawingRequest(
                        RowState.Edited,
                        2,
                        1,
                        1,
                        "background.jpg",
                        Convert.ToBase64String(new byte[] { 0 }),
                        new List<UpdateDrawingRequest>
                        {
                            new(
                                RowState.Added,
                                new DrawingObject
                                {
                                    DrawingType = 0,
                                    DrawingData = "DrawingData1",
                                    TargetMonth = new DateOnly(2023, 08, 01)
                                })
                        }),
                    new BatchUpdateResults<DrawingObjectResponse>
                    {
                        new()
                        {
                            RowState = RowState.Added,
                            DbUpdateStatus = DbUpdateStatus.Success,
                            Data = new DrawingObjectResponse
                            {
                                DrawingType = 0,
                                DrawingData = "DrawingData1",
                                TargetMonth = "2023-08",
                                RowVersion = Convert.ToBase64String(new byte[] { 0 })
                            }
                        }
                    },
                    string.Empty)
                    .SetName("追加する描画オブジェクト情報が正しく場合、データを追加すると、追加された描画オブジェクトを返す");

                yield return new TestCaseData(
                        new BatchUpdateYieldDrawingRequest(
                            RowState.Edited,
                            2,
                            1,
                            1,
                            "background.jpg",
                            Convert.ToBase64String(new byte[] { 0 }),
                            new List<UpdateDrawingRequest>
                            {
                                new(
                                    RowState.Added,
                                    new DrawingObject
                                    {
                                        DrawingType = 0,
                                        DrawingData = "DrawingData1",
                                        TargetMonth = new DateOnly(2023, 08, 01)
                                    }),
                                new(
                                    RowState.Added,
                                    new DrawingObject
                                    {
                                        DrawingType = 0,
                                        DrawingData = "DrawingData1",
                                        TargetMonth = new DateOnly(2023, 08, 01)
                                    })
                            }),
                        new BatchUpdateResults<DrawingObjectResponse>(),
                        "record duplicate")
                    .SetName("重複している描画オブジェクトが複数ある場合、更新されず、エラーを返す。");

                yield return new TestCaseData(
                    new BatchUpdateYieldDrawingRequest(
                        RowState.Edited,
                        2,
                        1,
                        1,
                        "background.jpg",
                        Convert.ToBase64String(new byte[] { 0 }),
                        new List<UpdateDrawingRequest>
                        {
                            new(
                                RowState.Edited,
                                new DrawingObject
                                {
                                    Id = 1,
                                    YieldDrawingId = 2,
                                    DrawingType = 0,
                                    DrawingData = "DrawingData1",
                                    TargetMonth = TargetMonth,
                                    RowVersion = Convert.ToBase64String(new byte[] { 1 })
                                })
                        }),
                    new BatchUpdateResults<DrawingObjectResponse>
                    {
                        new()
                        {
                            RowState = RowState.Edited,
                            DbUpdateStatus = DbUpdateStatus.Error,
                            Data = new DrawingObjectResponse
                            {
                                Id = 1,
                                DrawingType = 0,
                                DrawingData = "DrawingData1",
                                TargetMonth = "07-2023",
                                RowVersion = Convert.ToBase64String(new byte[] { 1 })
                            }
                        }
                    },
                    "has been updated by other process")
                    .SetName("更新する描画オブジェクト情報が不正な場合、エラーを返す");

                yield return new TestCaseData(
                    new BatchUpdateYieldDrawingRequest(
                        RowState.Edited,
                        2,
                        1,
                        1,
                        "background.jpg",
                        Convert.ToBase64String(new byte[] { 0 }),
                        new List<UpdateDrawingRequest>
                        {
                            new(
                                RowState.Added,
                                new DrawingObject
                                {
                                    Id = 0,
                                    YieldDrawingId = 2,
                                    DrawingType = 0,
                                    DrawingData = "Construction",
                                    TargetMonth = new DateOnly(2023, 7, 1)
                                })
                        }),
                    new BatchUpdateResults<DrawingObjectResponse>
                    {
                        new()
                        {
                            RowState = RowState.Added,
                            DbUpdateStatus = DbUpdateStatus.Error,
                            Data = new DrawingObjectResponse
                            {
                                Id = 0,
                                DrawingType = 0,
                                DrawingData = "Construction",
                                TargetMonth = "2023-07"
                            }
                        }
                    },
                    "record duplicate").SetName("追加する描画オブジェクト情報が不正な場合、エラーを返す");

                yield return new TestCaseData(
                    new BatchUpdateYieldDrawingRequest(
                        RowState.Edited,
                        2,
                        1,
                        1,
                        "background.jpg",
                        Convert.ToBase64String(new byte[] { 0 }),
                        new List<UpdateDrawingRequest>
                        {
                            new(
                                RowState.Added,
                                new DrawingObject
                                {
                                    Id = 0,
                                    YieldDrawingId = 2,
                                    DrawingType = 0,
                                    DrawingData = "Construction",
                                    TargetMonth = new DateOnly(2023, 09, 01)
                                }),
                            new(
                                RowState.Edited,
                                new DrawingObject
                                {
                                    Id = 1,
                                    YieldDrawingId = 2,
                                    DrawingType = 0,
                                    DrawingData = "DrawingData2",
                                    TargetMonth = TargetMonth,
                                    RowVersion = Convert.ToBase64String(new byte[] { 0 })
                                })
                        }),
                    new BatchUpdateResults<DrawingObjectResponse>
                    {
                        new()
                        {
                            RowState = RowState.Added,
                            RowNumber = 0,
                            DbUpdateStatus = DbUpdateStatus.Success,
                            Data = new DrawingObjectResponse
                            {
                                DrawingType = 0,
                                DrawingData = "Construction",
                                TargetMonth = "2023-09",
                                RowVersion = Convert.ToBase64String(new byte[] { 0 })
                            }
                        },
                        new()
                        {
                            RowState = RowState.Edited,
                            RowNumber = 1,
                            DbUpdateStatus = DbUpdateStatus.Success,
                            Data = new DrawingObjectResponse
                            {
                                DrawingType = 0,
                                DrawingData = "DrawingData2",
                                TargetMonth = "2023-07",
                                RowVersion = Convert.ToBase64String(new byte[] { 0 })
                            }
                        }
                    },
                    string.Empty)
                    .SetName("更新する描画オブジェクト情報が正しく場合、データを更新すると、更新された描画オブジェクトを返す");
            }
        }

        /// <summary>
        /// YieldDrawingApiV1UpdateBatchAsync のテストです。
        /// </summary>
        /// <param name="request">データを更新する</param>
        /// <param name="expected">返却される結果比較用パラメータ</param>
        /// <param name="errorMessage">返却されるエラーメッセージ</param>
        [TestCaseSource(nameof(UpdateBatchObjectAsyncTestCases))]
        public async Task UpdateBatchObjectAsyncTest(
            BatchUpdateYieldDrawingRequest request,
            BatchUpdateResults<DrawingObjectResponse> expected,
            string errorMessage)
        {
            // テストの準備
            var mapConfig =
                new MapperConfiguration(cfg => cfg.AddProfiles(
                    new Profile[] { new DrawingObjectResponse.MappingProfile() }));
            var useCase = new DrawingObjectUseCase(
                new Mapper(mapConfig),
                new YieldManagementTargetRepository(_dbContext, new DateService()),
                new DrawingObjectRepository(_dbContext, new DateService()),
                new YieldDrawingRepository(_dbContext, new DateService()));

            var actual = await useCase.UpdateBatchObjectAsync(request);

            if (actual.UpdateDrawingObjectResults.Any(_ => _.DbUpdateStatus == DbUpdateStatus.Success))
            {
                actual.UpdateDrawingObjectResults.Should().BeEquivalentTo(expected, o => o.Excluding(s => s.Data!.Id));
            }
            else
            {
                foreach (var actualResults in actual.UpdateDrawingObjectResults)
                {
                    actualResults.EntityName.Should().Be("drawingObject");
                    actualResults.Message.Should().Be(errorMessage);
                }
            }
        }
    }
}
