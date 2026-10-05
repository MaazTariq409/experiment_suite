#!/usr/bin/env python3
"""Generate the enterprise .NET experiment suite scaffold from frozen task metadata."""
from __future__ import annotations

import textwrap
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
TASKS = ROOT / "Tasks"

PAIRS = [
    {
        "id": "TP01",
        "capability": "REST API + CRUD + validation",
        "a": {
            "name": "Equipment Maintenance API",
            "slug": "EquipmentMaintenance",
            "entity": "MaintenanceRecord",
            "route": "api/maintenance-records",
        },
        "b": {
            "name": "Supplier Contract API",
            "slug": "SupplierContract",
            "entity": "SupplierContract",
            "route": "api/supplier-contracts",
        },
        "kind": "crud",
    },
    {
        "id": "TP02",
        "capability": "Authorization + business rules",
        "a": {"name": "Project Access Policy", "slug": "ProjectAccess", "entity": "ProjectAccess"},
        "b": {"name": "Document Access Policy", "slug": "DocumentAccess", "entity": "DocumentAccess"},
        "kind": "authz",
    },
    {
        "id": "TP03",
        "capability": "Database query + aggregation",
        "a": {"name": "Invoice Aging", "slug": "InvoiceAging", "entity": "Invoice"},
        "b": {"name": "Purchase Order Fulfilment", "slug": "PurchaseOrderFulfilment", "entity": "PurchaseOrder"},
        "kind": "query",
    },
    {
        "id": "TP04",
        "capability": "State-transition workflow",
        "a": {"name": "Leave Approval", "slug": "LeaveApproval", "entity": "LeaveRequest"},
        "b": {"name": "Expense Reimbursement", "slug": "ExpenseReimbursement", "entity": "ExpenseClaim"},
        "kind": "workflow",
    },
    {
        "id": "TP05",
        "capability": "External API integration",
        "a": {"name": "Currency Rate Service", "slug": "CurrencyRate", "entity": "CurrencyRate"},
        "b": {"name": "Shipping Rate Service", "slug": "ShippingRate", "entity": "ShippingRate"},
        "kind": "integration",
    },
    {
        "id": "TP06",
        "capability": "File import + validation",
        "a": {"name": "Employee Import", "slug": "EmployeeImport", "entity": "Employee"},
        "b": {"name": "Product Catalog Import", "slug": "ProductCatalogImport", "entity": "Product"},
        "kind": "import",
    },
    {
        "id": "TP07",
        "capability": "Refactoring + defect correction",
        "a": {"name": "Notification Preferences", "slug": "NotificationPreferences", "entity": "NotificationPreference"},
        "b": {"name": "Alert Subscriptions", "slug": "AlertSubscriptions", "entity": "AlertSubscription"},
        "kind": "refactor",
    },
    {
        "id": "TP08",
        "capability": "Reporting + date logic",
        "a": {"name": "Resource Utilization", "slug": "ResourceUtilization", "entity": "ResourceAssignment"},
        "b": {"name": "Support Ticket SLA", "slug": "SupportTicketSla", "entity": "SupportTicket"},
        "kind": "report",
    },
]


def write(path: Path, content: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(textwrap.dedent(content).lstrip("\n"), encoding="utf-8")


def csproj_lib(name: str, refs: list[str] | None = None, framework: str = "net8.0") -> str:
    refs = refs or []
    items = "\n".join(
        f'    <ProjectReference Include="{r}" />' for r in refs
    )
    ref_block = f"\n  <ItemGroup>\n{items}\n  </ItemGroup>\n" if items else ""
    return f"""\
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>{framework}</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>{name}</RootNamespace>
    <AssemblyName>{name}</AssemblyName>
  </PropertyGroup>{ref_block}
</Project>
"""


def csproj_test(name: str, refs: list[str]) -> str:
    items = "\n".join(f'    <ProjectReference Include="{r}" />' for r in refs)
    return f"""\
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <RootNamespace>{name}</RootNamespace>
    <AssemblyName>{name}</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="FluentAssertions" Version="6.12.2" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.11" />
  </ItemGroup>
  <ItemGroup>
{items}
  </ItemGroup>
</Project>
"""


def shared_files() -> None:
    shared = ROOT / "src" / "Enterprise.Shared"
    write(
        shared / "Enterprise.Shared.csproj",
        """\
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net8.0</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
          </PropertyGroup>
        </Project>
        """,
    )
    write(
        shared / "Results" / "OperationResult.cs",
        """\
        namespace Enterprise.Shared.Results;

        public sealed class OperationResult
        {
            public bool Succeeded { get; init; }
            public string? ErrorCode { get; init; }
            public string? ErrorMessage { get; init; }
            public int StatusCode { get; init; }

            public static OperationResult Ok(int statusCode = 200) =>
                new() { Succeeded = true, StatusCode = statusCode };

            public static OperationResult Fail(string code, string message, int statusCode) =>
                new() { Succeeded = false, ErrorCode = code, ErrorMessage = message, StatusCode = statusCode };
        }

        public sealed class OperationResult<T>
        {
            public bool Succeeded { get; init; }
            public T? Value { get; init; }
            public string? ErrorCode { get; init; }
            public string? ErrorMessage { get; init; }
            public int StatusCode { get; init; }

            public static OperationResult<T> Ok(T value, int statusCode = 200) =>
                new() { Succeeded = true, Value = value, StatusCode = statusCode };

            public static OperationResult<T> Fail(string code, string message, int statusCode) =>
                new() { Succeeded = false, ErrorCode = code, ErrorMessage = message, StatusCode = statusCode };
        }
        """,
    )
    write(
        shared / "Persistence" / "IUnitOfWork.cs",
        """\
        namespace Enterprise.Shared.Persistence;

        public interface IUnitOfWork
        {
            Task SaveChangesAsync(CancellationToken cancellationToken = default);
        }
        """,
    )
    write(
        shared / "Security" / "ActorContext.cs",
        """\
        namespace Enterprise.Shared.Security;

        public sealed record ActorContext(string UserId, string Role, IReadOnlySet<string> Permissions);
        """,
    )
    write(
        shared / "Time" / "IClock.cs",
        """\
        namespace Enterprise.Shared.Time;

        public interface IClock
        {
            DateTimeOffset UtcNow { get; }
        }

        public sealed class SystemClock : IClock
        {
            public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
        }

        public sealed class FixedClock : IClock
        {
            public FixedClock(DateTimeOffset utcNow) => UtcNow = utcNow;
            public DateTimeOffset UtcNow { get; }
        }
        """,
    )


def crud_contracts(pair_id: str, variant: str, meta: dict) -> None:
    ns = f"{pair_id}{variant}.Contracts"
    base = TASKS / pair_id / variant / "Contracts"
    entity = meta["entity"]
    write(base / f"{ns}.csproj", csproj_lib(ns, [r"..\..\..\..\src\Enterprise.Shared\Enterprise.Shared.csproj"]))
    write(
        base / "Dtos.cs",
        f"""\
        namespace {ns};

        public sealed class {entity}CreateRequest
        {{
            public string Code {{ get; set; }} = string.Empty;
            public string Title {{ get; set; }} = string.Empty;
            public DateOnly ScheduledDate {{ get; set; }}
            public decimal EstimatedCost {{ get; set; }}
            public string Status {{ get; set; }} = "Draft";
        }}

        public sealed class {entity}UpdateRequest
        {{
            public string Title {{ get; set; }} = string.Empty;
            public DateOnly ScheduledDate {{ get; set; }}
            public decimal EstimatedCost {{ get; set; }}
            public string Status {{ get; set; }} = string.Empty;
        }}

        public sealed class {entity}Response
        {{
            public Guid Id {{ get; set; }}
            public string Code {{ get; set; }} = string.Empty;
            public string Title {{ get; set; }} = string.Empty;
            public DateOnly ScheduledDate {{ get; set; }}
            public decimal EstimatedCost {{ get; set; }}
            public string Status {{ get; set; }} = string.Empty;
            public DateTimeOffset CreatedAtUtc {{ get; set; }}
            public DateTimeOffset? UpdatedAtUtc {{ get; set; }}
        }}
        """,
    )
    write(
        base / f"I{entity}Service.cs",
        f"""\
        using Enterprise.Shared.Results;

        namespace {ns};

        /// <summary>
        /// FROZEN CONTRACT — participants implement this interface; do not rename members.
        /// Evaluation tests compile only against this contract.
        /// </summary>
        public interface I{entity}Service
        {{
            Task<OperationResult<{entity}Response>> CreateAsync({entity}CreateRequest request, CancellationToken cancellationToken = default);
            Task<OperationResult<{entity}Response>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
            Task<OperationResult<{entity}Response>> UpdateAsync(Guid id, {entity}UpdateRequest request, CancellationToken cancellationToken = default);
        }}
        """,
    )
    write(
        base / f"I{entity}Repository.cs",
        f"""\
        namespace {ns};

        /// <summary>
        /// FROZEN persistence contract. Participant implementations may use the provided
        /// in-memory repository or substitute an equivalent that preserves identity semantics.
        /// </summary>
        public interface I{entity}Repository
        {{
            Task<{entity}Response?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
            Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
            Task AddAsync({entity}Response entity, CancellationToken cancellationToken = default);
            Task UpdateAsync({entity}Response entity, CancellationToken cancellationToken = default);
        }}
        """,
    )


def crud_participant(pair_id: str, variant: str, meta: dict) -> None:
    ns = f"{pair_id}{variant}.Participant"
    contracts = f"{pair_id}{variant}.Contracts"
    entity = meta["entity"]
    base = TASKS / pair_id / variant / "Participant"
    write(
        base / f"{ns}.csproj",
        csproj_lib(
            ns,
            [
                rf"..\Contracts\{contracts}.csproj",
                r"..\..\..\..\src\Enterprise.Shared\Enterprise.Shared.csproj",
            ],
        ),
    )
    write(
        base / "README_PARTICIPANT.txt",
        f"""\
        TASK {pair_id}-{variant}: {meta['name']}
        ==================================================
        Implement I{entity}Service in Services/{entity}Service.cs.

        FROZEN (do not change):
        - Contracts project (interfaces + DTOs)
        - Route names documented in TASK_SPEC.md
        - Validation rules listed in TASK_SPEC.md

        You MAY:
        - implement service logic from scratch
        - add private helper methods/classes inside Participant
        - use the provided InMemory{entity}Repository

        You MAY NOT:
        - change DTO property names/types
        - change interface method signatures
        - access the Evaluation project (it is not in this package)
        """,
    )
    write(
        base / "Persistence" / f"InMemory{entity}Repository.cs",
        f"""\
        using {contracts};

        namespace {ns}.Persistence;

        public sealed class InMemory{entity}Repository : I{entity}Repository
        {{
            private readonly Dictionary<Guid, {entity}Response> _store = new();

            public Task<{entity}Response?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            {{
                _store.TryGetValue(id, out var value);
                return Task.FromResult(value);
            }}

            public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
            {{
                var exists = _store.Values.Any(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase));
                return Task.FromResult(exists);
            }}

            public Task AddAsync({entity}Response entity, CancellationToken cancellationToken = default)
            {{
                _store[entity.Id] = entity;
                return Task.CompletedTask;
            }}

            public Task UpdateAsync({entity}Response entity, CancellationToken cancellationToken = default)
            {{
                _store[entity.Id] = entity;
                return Task.CompletedTask;
            }}
        }}
        """,
    )
    write(
        base / "Services" / f"{entity}Service.cs",
        f"""\
        using Enterprise.Shared.Results;
        using Enterprise.Shared.Time;
        using {contracts};

        namespace {ns}.Services;

        /// <summary>
        /// PARTICIPANT IMPLEMENTATION AREA.
        /// Replace NotImplementedException bodies with full business logic.
        /// </summary>
        public sealed class {entity}Service : I{entity}Service
        {{
            private readonly I{entity}Repository _repository;
            private readonly IClock _clock;

            public {entity}Service(I{entity}Repository repository, IClock clock)
            {{
                _repository = repository;
                _clock = clock;
            }}

            public Task<OperationResult<{entity}Response>> CreateAsync({entity}CreateRequest request, CancellationToken cancellationToken = default)
            {{
                // TODO: validate required fields, unique Code, cost >= 0, status in allowed set; persist; return 201
                throw new NotImplementedException("Implement CreateAsync for {pair_id}-{variant}.");
            }}

            public Task<OperationResult<{entity}Response>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            {{
                // TODO: return 404 when missing; otherwise 200 with entity
                throw new NotImplementedException("Implement GetByIdAsync for {pair_id}-{variant}.");
            }}

            public Task<OperationResult<{entity}Response>> UpdateAsync(Guid id, {entity}UpdateRequest request, CancellationToken cancellationToken = default)
            {{
                // TODO: validate; reject Terminal status updates; return 404 when missing; return 200 on success
                throw new NotImplementedException("Implement UpdateAsync for {pair_id}-{variant}.");
            }}
        }}
        """,
    )


def crud_evaluation(pair_id: str, variant: str, meta: dict) -> None:
    ns = f"{pair_id}{variant}.Evaluation"
    contracts = f"{pair_id}{variant}.Contracts"
    participant = f"{pair_id}{variant}.Participant"
    entity = meta["entity"]
    base = TASKS / pair_id / variant / "Evaluation"
    write(
        base / f"{ns}.csproj",
        csproj_test(
            ns,
            [
                rf"..\Contracts\{contracts}.csproj",
                rf"..\Participant\{participant}.csproj",
                r"..\..\..\..\src\Enterprise.Shared\Enterprise.Shared.csproj",
            ],
        ),
    )
    write(
        base / "HIDDEN.txt",
        """\
        HIDDEN EVALUATION SUITE — not distributed to participants.
        Tests bind only to frozen Contracts and instantiate the participant service via DI-style construction.
        """,
    )
    write(
        base / f"{entity}ServiceTests.cs",
        f"""\
        using Enterprise.Shared.Time;
        using FluentAssertions;
        using Xunit;
        using {contracts};
        using {participant}.Persistence;
        using {participant}.Services;

        namespace {ns};

        public class {entity}ServiceTests
        {{
            private static I{entity}Service CreateSut(DateTimeOffset? now = null)
            {{
                var clock = new FixedClock(now ?? new DateTimeOffset(2026, 3, 15, 12, 0, 0, TimeSpan.Zero));
                return new {entity}Service(new InMemory{entity}Repository(), clock);
            }}

            private static {entity}CreateRequest ValidCreate(string code = "EQ-1001") => new()
            {{
                Code = code,
                Title = "Quarterly inspection",
                ScheduledDate = new DateOnly(2026, 4, 1),
                EstimatedCost = 250.50m,
                Status = "Draft"
            }};

            [Fact]
            public async Task Create_ValidRequest_ReturnsCreatedEntity()
            {{
                var sut = CreateSut();
                var result = await sut.CreateAsync(ValidCreate());
                result.Succeeded.Should().BeTrue();
                result.StatusCode.Should().Be(201);
                result.Value.Should().NotBeNull();
                result.Value!.Code.Should().Be("EQ-1001");
                result.Value.Id.Should().NotBe(Guid.Empty);
            }}

            [Fact]
            public async Task Create_MissingTitle_ReturnsValidationError()
            {{
                var sut = CreateSut();
                var request = ValidCreate();
                request.Title = " ";
                var result = await sut.CreateAsync(request);
                result.Succeeded.Should().BeFalse();
                result.StatusCode.Should().Be(400);
                result.ErrorCode.Should().Be("VALIDATION_ERROR");
            }}

            [Fact]
            public async Task Create_NegativeCost_ReturnsValidationError()
            {{
                var sut = CreateSut();
                var request = ValidCreate();
                request.EstimatedCost = -1m;
                var result = await sut.CreateAsync(request);
                result.Succeeded.Should().BeFalse();
                result.StatusCode.Should().Be(400);
            }}

            [Fact]
            public async Task Create_DuplicateCode_ReturnsConflict()
            {{
                var sut = CreateSut();
                (await sut.CreateAsync(ValidCreate())).Succeeded.Should().BeTrue();
                var result = await sut.CreateAsync(ValidCreate());
                result.Succeeded.Should().BeFalse();
                result.StatusCode.Should().Be(409);
                result.ErrorCode.Should().Be("DUPLICATE_CODE");
            }}

            [Fact]
            public async Task GetById_Missing_ReturnsNotFound()
            {{
                var sut = CreateSut();
                var result = await sut.GetByIdAsync(Guid.NewGuid());
                result.Succeeded.Should().BeFalse();
                result.StatusCode.Should().Be(404);
            }}

            [Fact]
            public async Task GetById_Existing_ReturnsEntity()
            {{
                var sut = CreateSut();
                var created = await sut.CreateAsync(ValidCreate());
                var result = await sut.GetByIdAsync(created.Value!.Id);
                result.Succeeded.Should().BeTrue();
                result.Value!.Title.Should().Be("Quarterly inspection");
            }}

            [Fact]
            public async Task Update_Existing_UpdatesMutableFields()
            {{
                var sut = CreateSut();
                var created = await sut.CreateAsync(ValidCreate());
                var result = await sut.UpdateAsync(created.Value!.Id, new {entity}UpdateRequest
                {{
                    Title = "Updated title",
                    ScheduledDate = new DateOnly(2026, 5, 1),
                    EstimatedCost = 300m,
                    Status = "Scheduled"
                }});
                result.Succeeded.Should().BeTrue();
                result.Value!.Title.Should().Be("Updated title");
                result.Value.Status.Should().Be("Scheduled");
                result.Value.Code.Should().Be("EQ-1001");
            }}

            [Fact]
            public async Task Update_Missing_ReturnsNotFound()
            {{
                var sut = CreateSut();
                var result = await sut.UpdateAsync(Guid.NewGuid(), new {entity}UpdateRequest
                {{
                    Title = "X",
                    ScheduledDate = new DateOnly(2026, 5, 1),
                    EstimatedCost = 10m,
                    Status = "Draft"
                }});
                result.Succeeded.Should().BeFalse();
                result.StatusCode.Should().Be(404);
            }}

            [Fact]
            public async Task Update_ClosedStatus_RejectsMutation()
            {{
                var sut = CreateSut();
                var created = await sut.CreateAsync(ValidCreate());
                (await sut.UpdateAsync(created.Value!.Id, new {entity}UpdateRequest
                {{
                    Title = "Close me",
                    ScheduledDate = new DateOnly(2026, 4, 1),
                    EstimatedCost = 250.50m,
                    Status = "Closed"
                }})).Succeeded.Should().BeTrue();

                var result = await sut.UpdateAsync(created.Value.Id, new {entity}UpdateRequest
                {{
                    Title = "Should fail",
                    ScheduledDate = new DateOnly(2026, 6, 1),
                    EstimatedCost = 1m,
                    Status = "Draft"
                }});
                result.Succeeded.Should().BeFalse();
                result.StatusCode.Should().Be(409);
                result.ErrorCode.Should().Be("INVALID_STATE");
            }}

            [Fact]
            public async Task Create_InvalidStatus_ReturnsValidationError()
            {{
                var sut = CreateSut();
                var request = ValidCreate();
                request.Status = "Unknown";
                var result = await sut.CreateAsync(request);
                result.Succeeded.Should().BeFalse();
                result.StatusCode.Should().Be(400);
            }}
        }}
        """,
    )


def generic_service_task(pair: dict, variant_key: str) -> None:
    """Generate non-CRUD tasks with frozen interface + stub + sample tests."""
    pair_id = pair["id"]
    variant = variant_key.upper()
    meta = pair[variant_key]
    kind = pair["kind"]
    ns_c = f"{pair_id}{variant}.Contracts"
    ns_p = f"{pair_id}{variant}.Participant"
    ns_e = f"{pair_id}{variant}.Evaluation"
    slug = meta["slug"]
    entity = meta["entity"]

    cbase = TASKS / pair_id / variant / "Contracts"
    pbase = TASKS / pair_id / variant / "Participant"
    ebase = TASKS / pair_id / variant / "Evaluation"

    write(cbase / f"{ns_c}.csproj", csproj_lib(ns_c, [r"..\..\..\..\src\Enterprise.Shared\Enterprise.Shared.csproj"]))
    write(
        pbase / f"{ns_p}.csproj",
        csproj_lib(ns_p, [rf"..\Contracts\{ns_c}.csproj", r"..\..\..\..\src\Enterprise.Shared\Enterprise.Shared.csproj"]),
    )
    write(
        ebase / f"{ns_e}.csproj",
        csproj_test(
            ns_e,
            [
                rf"..\Contracts\{ns_c}.csproj",
                rf"..\Participant\{ns_p}.csproj",
                r"..\..\..\..\src\Enterprise.Shared\Enterprise.Shared.csproj",
            ],
        ),
    )
    write(ebase / "HIDDEN.txt", "HIDDEN EVALUATION SUITE — not distributed to participants.\n")

    interface_body, stub_body, test_body, dto_body = kind_templates(kind, ns_c, ns_p, ns_e, slug, entity, pair_id, variant, meta["name"])
    write(cbase / "Models.cs", dto_body)
    write(cbase / f"I{slug}Service.cs", interface_body)
    write(pbase / "Services" / f"{slug}Service.cs", stub_body)
    write(pbase / "README_PARTICIPANT.txt", f"TASK {pair_id}-{variant}: {meta['name']}\nImplement I{slug}Service. Contracts are frozen.\n")
    write(ebase / f"{slug}ServiceTests.cs", test_body)


def kind_templates(kind, ns_c, ns_p, ns_e, slug, entity, pair_id, variant, name):
    if kind == "authz":
        dto = f"""\
        namespace {ns_c};

        public sealed record AccessRequest(string ActorUserId, string ActorRole, string ResourceId, string Action, string ResourceStatus);
        public sealed record AccessDecision(bool Allowed, string ReasonCode);
        """
        iface = f"""\
        using Enterprise.Shared.Results;
        namespace {ns_c};
        public interface I{slug}Service
        {{
            Task<OperationResult<AccessDecision>> AuthorizeAsync(AccessRequest request, CancellationToken cancellationToken = default);
        }}
        """
        stub = f"""\
        using Enterprise.Shared.Results;
        using {ns_c};
        namespace {ns_p}.Services;
        public sealed class {slug}Service : I{slug}Service
        {{
            public Task<OperationResult<AccessDecision>> AuthorizeAsync(AccessRequest request, CancellationToken cancellationToken = default)
                => throw new NotImplementedException("Implement authorization rules for {pair_id}-{variant}.");
        }}
        """
        tests = f"""\
        using FluentAssertions;
        using Xunit;
        using {ns_c};
        using {ns_p}.Services;
        namespace {ns_e};
        public class {slug}ServiceTests
        {{
            [Fact]
            public async Task Admin_CanApprove_OpenResource()
            {{
                var sut = new {slug}Service();
                var result = await sut.AuthorizeAsync(new AccessRequest("u1", "Admin", "R1", "Approve", "Open"));
                result.Succeeded.Should().BeTrue();
                result.Value!.Allowed.Should().BeTrue();
            }}

            [Fact]
            public async Task Viewer_CannotUpdate()
            {{
                var sut = new {slug}Service();
                var result = await sut.AuthorizeAsync(new AccessRequest("u2", "Viewer", "R1", "Update", "Open"));
                result.Succeeded.Should().BeTrue();
                result.Value!.Allowed.Should().BeFalse();
                result.Value.ReasonCode.Should().Be("FORBIDDEN");
            }}

            [Fact]
            public async Task ClosedResource_RejectsMutation()
            {{
                var sut = new {slug}Service();
                var result = await sut.AuthorizeAsync(new AccessRequest("u1", "Admin", "R1", "Update", "Closed"));
                result.Succeeded.Should().BeTrue();
                result.Value!.Allowed.Should().BeFalse();
                result.Value.ReasonCode.Should().Be("INVALID_STATE");
            }}

            [Fact]
            public async Task UnknownAction_ReturnsValidationError()
            {{
                var sut = new {slug}Service();
                var result = await sut.AuthorizeAsync(new AccessRequest("u1", "Admin", "R1", "Explode", "Open"));
                result.Succeeded.Should().BeFalse();
                result.StatusCode.Should().Be(400);
            }}
        }}
        """
        return iface, stub, tests, dto

    if kind == "query":
        dto = f"""\
        namespace {ns_c};
        public sealed record {entity}Row(string Id, string OwnerKey, DateOnly DueDate, decimal Amount, decimal PaidAmount);
        public sealed record AgingBucket(string Name, decimal TotalAmount, int Count);
        public sealed record AgingReport(IReadOnlyList<AgingBucket> Buckets, decimal GrandTotal);
        public sealed record AgingQuery(string? OwnerKey, DateOnly AsOfDate);
        """
        iface = f"""\
        using Enterprise.Shared.Results;
        namespace {ns_c};
        public interface I{slug}Service
        {{
            Task<OperationResult<AgingReport>> BuildReportAsync(IReadOnlyList<{entity}Row> source, AgingQuery query, CancellationToken cancellationToken = default);
        }}
        """
        stub = f"""\
        using Enterprise.Shared.Results;
        using {ns_c};
        namespace {ns_p}.Services;
        public sealed class {slug}Service : I{slug}Service
        {{
            public Task<OperationResult<AgingReport>> BuildReportAsync(IReadOnlyList<{entity}Row> source, AgingQuery query, CancellationToken cancellationToken = default)
                => throw new NotImplementedException("Implement aggregation for {pair_id}-{variant}.");
        }}
        """
        tests = f"""\
        using FluentAssertions;
        using Xunit;
        using {ns_c};
        using {ns_p}.Services;
        namespace {ns_e};
        public class {slug}ServiceTests
        {{
            private static List<{entity}Row> Sample() =>
            [
                new("1", "ACME", new DateOnly(2026, 1, 1), 100m, 0m),
                new("2", "ACME", new DateOnly(2026, 3, 1), 50m, 10m),
                new("3", "BETA", new DateOnly(2026, 3, 10), 20m, 20m)
            ];

            [Fact]
            public async Task BuildsDeterministicBuckets()
            {{
                var sut = new {slug}Service();
                var result = await sut.BuildReportAsync(Sample(), new AgingQuery("ACME", new DateOnly(2026, 3, 15)));
                result.Succeeded.Should().BeTrue();
                result.Value!.Buckets.Should().NotBeEmpty();
                result.Value.GrandTotal.Should().Be(140m);
            }}

            [Fact]
            public async Task ZeroOutstanding_ExcludedFromGrandTotal()
            {{
                var sut = new {slug}Service();
                var result = await sut.BuildReportAsync(Sample(), new AgingQuery("BETA", new DateOnly(2026, 3, 15)));
                result.Succeeded.Should().BeTrue();
                result.Value!.GrandTotal.Should().Be(0m);
            }}

            [Fact]
            public async Task EmptySource_ReturnsEmptyReport()
            {{
                var sut = new {slug}Service();
                var result = await sut.BuildReportAsync([], new AgingQuery(null, new DateOnly(2026, 3, 15)));
                result.Succeeded.Should().BeTrue();
                result.Value!.GrandTotal.Should().Be(0m);
            }}
        }}
        """
        return iface, stub, tests, dto

    if kind == "workflow":
        dto = f"""\
        namespace {ns_c};
        public sealed record TransitionRequest(Guid Id, string CurrentStatus, string TargetStatus, string ActorRole, string? Comment);
        public sealed record TransitionResult(string NewStatus, bool Applied, string ReasonCode);
        """
        iface = f"""\
        using Enterprise.Shared.Results;
        namespace {ns_c};
        public interface I{slug}Service
        {{
            Task<OperationResult<TransitionResult>> TransitionAsync(TransitionRequest request, CancellationToken cancellationToken = default);
        }}
        """
        stub = f"""\
        using Enterprise.Shared.Results;
        using {ns_c};
        namespace {ns_p}.Services;
        public sealed class {slug}Service : I{slug}Service
        {{
            public Task<OperationResult<TransitionResult>> TransitionAsync(TransitionRequest request, CancellationToken cancellationToken = default)
                => throw new NotImplementedException("Implement workflow transitions for {pair_id}-{variant}.");
        }}
        """
        tests = f"""\
        using FluentAssertions;
        using Xunit;
        using {ns_c};
        using {ns_p}.Services;
        namespace {ns_e};
        public class {slug}ServiceTests
        {{
            [Fact]
            public async Task Submitted_To_Approved_ByManager_Succeeds()
            {{
                var sut = new {slug}Service();
                var result = await sut.TransitionAsync(new TransitionRequest(Guid.NewGuid(), "Submitted", "Approved", "Manager", "ok"));
                result.Succeeded.Should().BeTrue();
                result.Value!.Applied.Should().BeTrue();
                result.Value.NewStatus.Should().Be("Approved");
            }}

            [Fact]
            public async Task InvalidTransition_IsRejected()
            {{
                var sut = new {slug}Service();
                var result = await sut.TransitionAsync(new TransitionRequest(Guid.NewGuid(), "Draft", "Approved", "Manager", null));
                result.Succeeded.Should().BeTrue();
                result.Value!.Applied.Should().BeFalse();
                result.Value.ReasonCode.Should().Be("INVALID_TRANSITION");
            }}

            [Fact]
            public async Task Employee_CannotApprove()
            {{
                var sut = new {slug}Service();
                var result = await sut.TransitionAsync(new TransitionRequest(Guid.NewGuid(), "Submitted", "Approved", "Employee", "no"));
                result.Succeeded.Should().BeTrue();
                result.Value!.Applied.Should().BeFalse();
                result.Value.ReasonCode.Should().Be("FORBIDDEN");
            }}
        }}
        """
        return iface, stub, tests, dto

    if kind == "integration":
        dto = f"""\
        namespace {ns_c};
        public sealed record ExternalQuoteRequest(string From, string To, decimal Amount);
        public sealed record ExternalQuoteRaw(string From, string To, decimal Rate, bool TimedOut, bool Failed);
        public sealed record QuoteResponse(string From, string To, decimal Rate, decimal ConvertedAmount, string Source);
        public interface IExternalQuoteClient
        {{
            Task<ExternalQuoteRaw> GetQuoteAsync(ExternalQuoteRequest request, CancellationToken cancellationToken = default);
        }}
        """
        iface = f"""\
        using Enterprise.Shared.Results;
        namespace {ns_c};
        public interface I{slug}Service
        {{
            Task<OperationResult<QuoteResponse>> GetQuoteAsync(ExternalQuoteRequest request, CancellationToken cancellationToken = default);
        }}
        """
        stub = f"""\
        using Enterprise.Shared.Results;
        using {ns_c};
        namespace {ns_p}.Services;
        public sealed class {slug}Service : I{slug}Service
        {{
            private readonly IExternalQuoteClient _client;
            public {slug}Service(IExternalQuoteClient client) => _client = client;
            public Task<OperationResult<QuoteResponse>> GetQuoteAsync(ExternalQuoteRequest request, CancellationToken cancellationToken = default)
                => throw new NotImplementedException("Implement external mapping/error handling for {pair_id}-{variant}.");
        }}
        """
        tests = f"""\
        using FluentAssertions;
        using Xunit;
        using {ns_c};
        using {ns_p}.Services;
        namespace {ns_e};
        file sealed class FakeClient(ExternalQuoteRaw raw) : IExternalQuoteClient
        {{
            public Task<ExternalQuoteRaw> GetQuoteAsync(ExternalQuoteRequest request, CancellationToken cancellationToken = default)
                => Task.FromResult(raw);
        }}
        public class {slug}ServiceTests
        {{
            [Fact]
            public async Task MapsSuccessfulExternalResponse()
            {{
                var sut = new {slug}Service(new FakeClient(new ExternalQuoteRaw("USD", "EUR", 0.9m, false, false)));
                var result = await sut.GetQuoteAsync(new ExternalQuoteRequest("USD", "EUR", 100m));
                result.Succeeded.Should().BeTrue();
                result.Value!.ConvertedAmount.Should().Be(90m);
            }}

            [Fact]
            public async Task Timeout_ReturnsGatewayTimeout()
            {{
                var sut = new {slug}Service(new FakeClient(new ExternalQuoteRaw("USD", "EUR", 0m, true, false)));
                var result = await sut.GetQuoteAsync(new ExternalQuoteRequest("USD", "EUR", 100m));
                result.Succeeded.Should().BeFalse();
                result.StatusCode.Should().Be(504);
            }}

            [Fact]
            public async Task FailedExternal_ReturnsBadGateway()
            {{
                var sut = new {slug}Service(new FakeClient(new ExternalQuoteRaw("USD", "EUR", 0m, false, true)));
                var result = await sut.GetQuoteAsync(new ExternalQuoteRequest("USD", "EUR", 100m));
                result.Succeeded.Should().BeFalse();
                result.StatusCode.Should().Be(502);
            }}
        }}
        """
        return iface, stub, tests, dto

    if kind == "import":
        dto = f"""\
        namespace {ns_c};
        public sealed record ImportRow(int LineNumber, string Raw);
        public sealed record ImportSummary(int Accepted, int Rejected, IReadOnlyList<string> Errors);
        public sealed record {entity}Record(string Key, string Name, decimal Value);
        """
        iface = f"""\
        using Enterprise.Shared.Results;
        namespace {ns_c};
        public interface I{slug}Service
        {{
            Task<OperationResult<ImportSummary>> ImportAsync(IReadOnlyList<ImportRow> rows, CancellationToken cancellationToken = default);
        }}
        """
        stub = f"""\
        using Enterprise.Shared.Results;
        using {ns_c};
        namespace {ns_p}.Services;
        public sealed class {slug}Service : I{slug}Service
        {{
            public Task<OperationResult<ImportSummary>> ImportAsync(IReadOnlyList<ImportRow> rows, CancellationToken cancellationToken = default)
                => throw new NotImplementedException("Implement file import validation for {pair_id}-{variant}.");
        }}
        """
        tests = f"""\
        using FluentAssertions;
        using Xunit;
        using {ns_c};
        using {ns_p}.Services;
        namespace {ns_e};
        public class {slug}ServiceTests
        {{
            [Fact]
            public async Task AcceptsValidCsvRows()
            {{
                var sut = new {slug}Service();
                var rows = new[] {{ new ImportRow(1, "K1,Name,10.5"), new ImportRow(2, "K2,Name2,3") }};
                var result = await sut.ImportAsync(rows);
                result.Succeeded.Should().BeTrue();
                result.Value!.Accepted.Should().Be(2);
                result.Value.Rejected.Should().Be(0);
            }}

            [Fact]
            public async Task RejectsMalformedRows_ButContinues()
            {{
                var sut = new {slug}Service();
                var rows = new[] {{ new ImportRow(1, "K1,Name,10.5"), new ImportRow(2, "BAD") }};
                var result = await sut.ImportAsync(rows);
                result.Succeeded.Should().BeTrue();
                result.Value!.Accepted.Should().Be(1);
                result.Value.Rejected.Should().Be(1);
                result.Value.Errors.Should().Contain(e => e.Contains("2"));
            }}

            [Fact]
            public async Task DuplicateKey_IsRejected()
            {{
                var sut = new {slug}Service();
                var rows = new[] {{ new ImportRow(1, "K1,Name,10.5"), new ImportRow(2, "K1,Name2,3") }};
                var result = await sut.ImportAsync(rows);
                result.Succeeded.Should().BeTrue();
                result.Value!.Accepted.Should().Be(1);
                result.Value.Rejected.Should().Be(1);
            }}
        }}
        """
        return iface, stub, tests, dto

    if kind == "refactor":
        dto = f"""\
        namespace {ns_c};
        public sealed record PreferenceDto(string UserId, string Channel, bool Enabled, string Frequency);
        """
        iface = f"""\
        using Enterprise.Shared.Results;
        namespace {ns_c};
        public interface I{slug}Service
        {{
            Task<OperationResult<PreferenceDto>> UpsertAsync(PreferenceDto preference, CancellationToken cancellationToken = default);
            Task<OperationResult<PreferenceDto>> GetAsync(string userId, string channel, CancellationToken cancellationToken = default);
        }}
        """
        stub = f"""\
        using Enterprise.Shared.Results;
        using {ns_c};
        namespace {ns_p}.Services;

        /// <summary>
        /// LEGACY STARTER with intentional defects/smells. Participants must fix behavior
        /// and improve structure while preserving the frozen interface.
        /// </summary>
        public sealed class {slug}Service : I{slug}Service
        {{
            private readonly Dictionary<string, PreferenceDto> _data = new();

            public Task<OperationResult<PreferenceDto>> UpsertAsync(PreferenceDto preference, CancellationToken cancellationToken = default)
            {{
                // Defect: accepts blank UserId; Smell: duplicated key logic / magic strings
                var key = preference.UserId + ":" + preference.Channel;
                _data[key] = preference;
                return Task.FromResult(OperationResult<PreferenceDto>.Ok(preference));
            }}

            public Task<OperationResult<PreferenceDto>> GetAsync(string userId, string channel, CancellationToken cancellationToken = default)
            {{
                var key = userId + ":" + channel;
                if (_data.TryGetValue(key, out var value))
                    return Task.FromResult(OperationResult<PreferenceDto>.Ok(value));
                // Defect: returns 400 instead of 404
                return Task.FromResult(OperationResult<PreferenceDto>.Fail("BAD_REQUEST", "missing", 400));
            }}
        }}
        """
        tests = f"""\
        using FluentAssertions;
        using Xunit;
        using {ns_c};
        using {ns_p}.Services;
        namespace {ns_e};
        public class {slug}ServiceTests
        {{
            [Fact]
            public async Task Upsert_RejectsBlankUserId()
            {{
                var sut = new {slug}Service();
                var result = await sut.UpsertAsync(new PreferenceDto(" ", "Email", true, "Daily"));
                result.Succeeded.Should().BeFalse();
                result.StatusCode.Should().Be(400);
            }}

            [Fact]
            public async Task Get_Missing_Returns404()
            {{
                var sut = new {slug}Service();
                var result = await sut.GetAsync("u1", "Email");
                result.Succeeded.Should().BeFalse();
                result.StatusCode.Should().Be(404);
            }}

            [Fact]
            public async Task Upsert_ThenGet_RoundTrips()
            {{
                var sut = new {slug}Service();
                await sut.UpsertAsync(new PreferenceDto("u1", "Email", true, "Daily"));
                var result = await sut.GetAsync("u1", "Email");
                result.Succeeded.Should().BeTrue();
                result.Value!.Frequency.Should().Be("Daily");
            }}
        }}
        """
        return iface, stub, tests, dto

    # report
    dto = f"""\
    namespace {ns_c};
    public sealed record ReportRow(string ResourceId, DateOnly Start, DateOnly End, decimal Units);
    public sealed record ReportQuery(DateOnly From, DateOnly To);
    public sealed record ReportLine(string ResourceId, decimal TotalUnits, decimal UtilizationPercent);
    public sealed record ReportResult(IReadOnlyList<ReportLine> Lines, decimal OverallUtilizationPercent);
    """
    iface = f"""\
    using Enterprise.Shared.Results;
    namespace {ns_c};
    public interface I{slug}Service
    {{
        Task<OperationResult<ReportResult>> BuildAsync(IReadOnlyList<ReportRow> source, ReportQuery query, CancellationToken cancellationToken = default);
    }}
    """
    stub = f"""\
    using Enterprise.Shared.Results;
    using {ns_c};
    namespace {ns_p}.Services;
    public sealed class {slug}Service : I{slug}Service
    {{
        public Task<OperationResult<ReportResult>> BuildAsync(IReadOnlyList<ReportRow> source, ReportQuery query, CancellationToken cancellationToken = default)
            => throw new NotImplementedException("Implement reporting/date logic for {pair_id}-{variant}.");
    }}
    """
    tests = f"""\
    using FluentAssertions;
    using {ns_c};
    using {ns_p}.Services;
    namespace {ns_e};
    public class {slug}ServiceTests
    {{
        [Fact]
        public async Task InvalidDateRange_ReturnsValidationError()
        {{
            var sut = new {slug}Service();
            var result = await sut.BuildAsync([], new ReportQuery(new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 1)));
            result.Succeeded.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }}

        [Fact]
        public async Task EmptyPeriod_ReturnsZeroUtilization()
        {{
            var sut = new {slug}Service();
            var result = await sut.BuildAsync([], new ReportQuery(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31)));
            result.Succeeded.Should().BeTrue();
            result.Value!.OverallUtilizationPercent.Should().Be(0m);
        }}

        [Fact]
        public async Task AggregatesByResource()
        {{
            var sut = new {slug}Service();
            var rows = new[]
            {{
                new ReportRow("R1", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 5), 5m),
                new ReportRow("R1", new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 12), 3m),
                new ReportRow("R2", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 2), 2m)
            }};
            var result = await sut.BuildAsync(rows, new ReportQuery(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31)));
            result.Succeeded.Should().BeTrue();
            result.Value!.Lines.Should().HaveCount(2);
        }}
    }}
    """
    return iface, stub, tests, dto


def task_spec(pair: dict, variant_key: str) -> None:
    pair_id = pair["id"]
    variant = variant_key.upper()
    meta = pair[variant_key]
    other = pair["b" if variant_key == "a" else "a"]
    path = ROOT / "task_specs" / f"{pair_id}_{variant}_{meta['slug']}.md"
    write(
        path,
        f"""\
        # {pair_id}-{variant}: {meta['name']}

        | Field | Value |
        |---|---|
        | Task pair | {pair_id} |
        | Variant | {variant} |
        | Capability | {pair['capability']} |
        | Matched counterpart | {pair_id}-{"B" if variant_key=="a" else "A"} ({other['name']}) |
        | Implementation model | Implement frozen service contract inside supplied project |
        | Standalone program? | No — work occurs inside the supplied enterprise baseline |
        | Hidden tests | Distributed only in Evaluation project (not shown to participants) |

        ## Enterprise-oriented characteristics
        - Layered structure (contracts / services / persistence abstractions)
        - Explicit business rules and validation
        - Deterministic error codes and status semantics
        - Domain scenario with enterprise identifiers and lifecycle states
        - Evaluation against a predefined requirement-to-test matrix

        ## Participant instructions (summary)
        1. Open the Participant project for this variant.
        2. Read frozen Contracts (do not modify).
        3. Implement service logic from scratch (except TP07, which starts from defective legacy code).
        4. Build the Participant project until it compiles.
        5. Submit when you believe the requirements are satisfied, or when the time limit is reached.

        ## Completion rule
        Task completion is participant-declared submission **or** reaching the maximum allotted time.
        Hidden tests may still fail after submission; failing tests do **not** block the timer stop event.

        ## Success criteria (evaluation)
        Functional success is measured post-hoc by the hidden test suite (pass/fail count).
        Structural quality is measured by the frozen static-analysis configuration on participant-authored files.
        """,
    )


def reference_solution_tp01() -> None:
    """Private reference implementation for TP01-A used to verify the evaluator."""
    base = ROOT / "reference_solutions" / "PRIVATE_DO_NOT_DISTRIBUTE" / "TP01A"
    write(
        base / "MaintenanceRecordService.Reference.cs",
        """\
        // REFERENCE ONLY — not distributed to participants.
        using Enterprise.Shared.Results;
        using Enterprise.Shared.Time;
        using TP01A.Contracts;

        namespace TP01A.Reference;

        public sealed class MaintenanceRecordServiceReference : IMaintenanceRecordService
        {
            private static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
            { "Draft", "Scheduled", "InProgress", "Closed" };

            private readonly IMaintenanceRecordRepository _repository;
            private readonly IClock _clock;

            public MaintenanceRecordServiceReference(IMaintenanceRecordRepository repository, IClock clock)
            {
                _repository = repository;
                _clock = clock;
            }

            public async Task<OperationResult<MaintenanceRecordResponse>> CreateAsync(MaintenanceRecordCreateRequest request, CancellationToken cancellationToken = default)
            {
                var validation = Validate(request.Title, request.EstimatedCost, request.Status, request.Code);
                if (validation is not null) return validation;

                if (await _repository.CodeExistsAsync(request.Code, cancellationToken))
                    return OperationResult<MaintenanceRecordResponse>.Fail("DUPLICATE_CODE", "Code already exists.", 409);

                var entity = new MaintenanceRecordResponse
                {
                    Id = Guid.NewGuid(),
                    Code = request.Code.Trim(),
                    Title = request.Title.Trim(),
                    ScheduledDate = request.ScheduledDate,
                    EstimatedCost = request.EstimatedCost,
                    Status = request.Status,
                    CreatedAtUtc = _clock.UtcNow
                };
                await _repository.AddAsync(entity, cancellationToken);
                return OperationResult<MaintenanceRecordResponse>.Ok(entity, 201);
            }

            public async Task<OperationResult<MaintenanceRecordResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            {
                var entity = await _repository.GetByIdAsync(id, cancellationToken);
                return entity is null
                    ? OperationResult<MaintenanceRecordResponse>.Fail("NOT_FOUND", "Record not found.", 404)
                    : OperationResult<MaintenanceRecordResponse>.Ok(entity);
            }

            public async Task<OperationResult<MaintenanceRecordResponse>> UpdateAsync(Guid id, MaintenanceRecordUpdateRequest request, CancellationToken cancellationToken = default)
            {
                var existing = await _repository.GetByIdAsync(id, cancellationToken);
                if (existing is null)
                    return OperationResult<MaintenanceRecordResponse>.Fail("NOT_FOUND", "Record not found.", 404);

                if (string.Equals(existing.Status, "Closed", StringComparison.OrdinalIgnoreCase))
                    return OperationResult<MaintenanceRecordResponse>.Fail("INVALID_STATE", "Closed records cannot be updated.", 409);

                var validation = Validate(request.Title, request.EstimatedCost, request.Status, code: null);
                if (validation is not null) return validation;

                existing.Title = request.Title.Trim();
                existing.ScheduledDate = request.ScheduledDate;
                existing.EstimatedCost = request.EstimatedCost;
                existing.Status = request.Status;
                existing.UpdatedAtUtc = _clock.UtcNow;
                await _repository.UpdateAsync(existing, cancellationToken);
                return OperationResult<MaintenanceRecordResponse>.Ok(existing);
            }

            private static OperationResult<MaintenanceRecordResponse>? Validate(string title, decimal cost, string status, string? code)
            {
                if (code is not null && string.IsNullOrWhiteSpace(code))
                    return OperationResult<MaintenanceRecordResponse>.Fail("VALIDATION_ERROR", "Code is required.", 400);
                if (string.IsNullOrWhiteSpace(title))
                    return OperationResult<MaintenanceRecordResponse>.Fail("VALIDATION_ERROR", "Title is required.", 400);
                if (cost < 0)
                    return OperationResult<MaintenanceRecordResponse>.Fail("VALIDATION_ERROR", "Cost must be >= 0.", 400);
                if (!Allowed.Contains(status))
                    return OperationResult<MaintenanceRecordResponse>.Fail("VALIDATION_ERROR", "Invalid status.", 400);
                return null;
            }
        }
        """,
    )


def docs() -> None:
    write(
        ROOT / "README.md",
        """\
        # Enterprise .NET Experiment Suite

        Reproducibility package for the controlled crossover study of AI-assisted enterprise-oriented .NET development (TP01–TP08, variants A/B).

        ## What this package proves to reviewers

        1. Tasks are **not** standalone toy programs. Each variant is completed **inside a supplied enterprise project** with frozen contracts, DTOs, and persistence abstractions.
        2. Participants implement functionality **largely from scratch** in marked service classes (TP07 starts from defective legacy code by design).
        3. Independently written solutions remain testable because evaluation binds only to **frozen interface contracts**, not to participant-specific internal design.
        4. Hidden tests are separated from the participant package.

        ## Layout

        ```
        experiment_suite/
          src/Enterprise.Shared/           Shared result/time/security primitives
          Tasks/TP0x/{A|B}/
            Contracts/                     FROZEN — interfaces + DTOs
            Participant/                   What developers edit
            Evaluation/                    HIDDEN tests (reviewer/repro package)
          task_specs/                      Participant-facing requirement sheets
          docs/                            Methodological documentation
          reference_solutions/PRIVATE_...  Private oracles (not for participants)
          scripts/                         Packaging and evaluation helpers
        ```

        ## Build

        ```powershell
        dotnet build EnterpriseTasks.sln
        ```

        ## Evaluate one variant (example TP01-A)

        ```powershell
        ./scripts/evaluate.ps1 -Task TP01 -Variant A
        ```

        The script runs only the Evaluation project for that variant and prints failed-test counts.

        ## Participant package vs full package

        | Content | Participant zip | Reviewer/repro zip |
        |---|---|---|
        | Contracts + Participant stubs + task specs | Yes | Yes |
        | Evaluation hidden tests | No | Yes |
        | Reference solutions | No | Optional / private |
        | Analyzer config | Manifest only | Full config |

        Use `./scripts/pack_participant.ps1` to emit participant zips without Evaluation folders.

        ## Related manuscript claims this package supports

        - Enterprise-oriented operationalization
        - Interface-contract based automated correctness
        - Completion rule independent of test pass/fail
        - Matched A/B variants with shared capability, different domain labels
        """,
    )

    write(
        ROOT / "docs" / "01_ENTERPRISE_ORIENTATION.md",
        """\
        # Operational definition: enterprise-oriented .NET tasks

        ## Definition used in this study

        A task is treated as **enterprise-oriented** when it requires implementation work inside a supplied multi-layer .NET application baseline and includes **at least four** of the following characteristics:

        1. **Layered architecture** — separation of contracts/API surface, domain/service logic, and persistence/integration abstractions.
        2. **Business-rule enforcement** — status lifecycles, authorization predicates, validation thresholds, or aggregation rules that are not reducible to a single algorithmic puzzle.
        3. **Deterministic enterprise responses** — explicit success/error codes analogous to service-layer/HTTP semantics used in business systems.
        4. **Domain scenario** — named business entities, identifiers, and operational context (maintenance, contracts, access policy, fulfilment, SLA, etc.).
        5. **Integration or persistence boundary** — repository, file import, or external-service abstraction provided by the baseline.
        6. **Quality constraints beyond “code runs”** — hidden functional tests plus static-analysis outcomes on the submitted implementation.

        ## Why these are not ordinary standalone exercises

        Participants do **not** create a new console app from a blank folder.
        They receive a predefined solution structure and must implement service behavior that plugs into frozen contracts used by the evaluation harness.

        Ordinary programming exercises typically:
        - start from a blank file or single-function template;
        - omit authorization/workflow/persistence boundaries;
        - evaluate only stdout or a few public tests visible to the solver.

        This suite requires enterprise-style boundaries even when the amount of code is intentionally bounded for experimental control.

        ## Standalone vs enhancement

        | Question | Answer in this suite |
        |---|---|
        | Standalone greenfield application? | No |
        | Modify an existing large production monolith? | No |
        | Implement missing business functionality inside a supplied baseline? | **Yes** |
        | Starting codebase contents | Frozen contracts, DTOs, repository abstractions, in-memory persistence helpers, task specification |
        | What participants write | Service implementations (and helpers) satisfying the contract |

        This is best described as **constrained greenfield implementation inside a supplied enterprise scaffold**, not as maintenance of a legacy production system (except TP07, which is refactoring of defective supplied code).
        """,
    )

    write(
        ROOT / "docs" / "02_EVALUATION_MODEL.md",
        """\
        # How independently written solutions are tested

        ## Reviewer question

        “How can the same test suite run against implementations created independently by different developers?”

        ## Answer

        Because tests never depend on participants’ private class designs.
        They depend only on **frozen contracts**.

        ```
        Participant writes code here          Evaluator binds here
        -----------------------------         -----------------------
        Participant/Services/*.cs      --->   Contracts/I*Service.cs
                                              Contracts DTO shapes
                                              StatusCode / ErrorCode semantics
        ```

        ### Fixed artifacts (identical for every participant)
        - Interface names and method signatures
        - DTO property names and types
        - Allowed status values / business-rule tables in the task spec
        - Error code strings required by tests (`VALIDATION_ERROR`, `NOT_FOUND`, ...)
        - Repository abstractions / external client abstractions where applicable

        ### Free artifacts (may differ across participants)
        - Private helper methods
        - Internal decomposition
        - Validation implementation style
        - Additional files under Participant (as long as contracts remain unchanged)

        ## Technical binding

        Evaluation projects:
        1. reference `*.Contracts` and `*.Participant`;
        2. construct the participant service with provided dependencies (in-memory repository, fake external client, fixed clock);
        3. invoke interface methods;
        4. assert on `OperationResult` success/failure, status codes, and DTO field values.

        No source-code text matching is used for correctness scoring.

        ## Visibility policy

        - Participants receive **Contracts + Participant + task spec**.
        - Participants do **not** receive Evaluation projects.
        - Optional visible smoke examples may be provided in future packs; the confirmatory suite remains hidden.
        - AI tools in the participant environment therefore cannot read hidden tests unless a packaging error occurs.

        ## Build failure / incomplete submission policy

        | Situation | Scoring rule |
        |---|---|
        | Project does not compile | All hidden tests count as failed |
        | Interface not implemented / NotImplementedException | Corresponding tests fail |
        | Partial implementation | Failed tests counted individually |
        | Submission after time limit | Same evaluation applied; completion time censored at limit |

        ## Test-to-requirement traceability

        Each Evaluation fact method maps to an explicit requirement in `task_specs/*.md`.
        Example (TP01): `Create_DuplicateCode_ReturnsConflict` ↔ “Reject duplicate business codes with conflict semantics.”
        """,
    )

    write(
        ROOT / "docs" / "03_COMPLETION_CRITERIA.md",
        """\
        # Completion criteria and stopping rule

        ## Operational rule (frozen)

        A task observation ends when **either**:

        1. the participant **declares completion and submits** the Participant project for evaluation; **or**
        2. the participant reaches the **predefined maximum task time**.

        The timer does **not** wait for all hidden tests to pass.

        ## Why this matters

        Completion time and test failures are intentionally separable outcomes:
        - a fast submission may still have failing tests;
        - a slow submission may pass all tests.

        This preserves the productivity outcome as an implementation-time measure and the correctness outcome as a post-hoc automated measure.

        ## What “task complete” does **not** mean
        - It does not mean “all tests passed”.
        - It does not mean “AI confirmed the solution”.
        - It does not mean “researcher manually accepted the code”.

        ## Recommended session reporting fields
        - `task_start`, `task_end`
        - `stop_reason` ∈ {`participant_submit`, `time_limit`}
        - `build_succeeded` (bool)
        - `test_failures` (int)
        - `tests_total` (int)
        """,
    )

    write(
        ROOT / "docs" / "04_PARTICIPANT_PACKAGE.md",
        """\
        # Participant package contents

        For each assigned variant, the participant zip contains:

        ```
        TP0xY/
          Contracts/                 (read-only)
          Participant/               (implementation area)
          TASK_SPEC.md               (requirements & acceptance criteria)
          README_PARTICIPANT.txt
        ```

        Plus once per session:
        ```
        Enterprise.Shared/
        ENVIRONMENT.md               (.NET SDK, IDE, permitted tooling)
        ```

        Explicitly excluded from participant zips:
        - `Evaluation/`
        - `reference_solutions/`
        - answer keys / scoring rubrics beyond the task spec

        ## AI-condition note

        Because hidden tests are absent from the workspace, the AI assistant can read only participant-visible artifacts (specs, contracts, stubs), not the confirmatory oracle.
        """,
    )

    write(
        ROOT / "docs" / "05_STATIC_ANALYSIS.md",
        """\
        # Static analysis configuration (placeholder for freeze)

        Fill with the exact values used during data collection before reviewer release:

        | Item | Value |
        |---|---|
        | Tool | [e.g., SonarAnalyzer.CSharp / Roslynator / custom] |
        | Version | [x.y.z] |
        | Ruleset path | `analyzer/Ruleset.ruleset` |
        | Scope | Participant-authored `.cs` files only |
        | Code smell definition | Rule IDs listed in `analyzer/smell_rules.txt` |
        | Cyclomatic complexity aggregation | Mean of method-level CC over analyzed methods in scope |
        | Maintainability Index | [tool formula / Visual Studio MI] with documented parameters |

        Do not invent versions. Replace bracketed fields with the laboratory’s actual frozen toolchain.
        """,
    )


def solution_file(projects: list[str]) -> None:
    configs = []
    for i, p in enumerate(projects):
        guid = f"{{{(i+1):08X}-0000-4000-8000-000000000000}}"
        configs.append((guid, p))
    lines = ["Microsoft Visual Studio Solution File, Format Version 12.00", "# Dotnet CLI generated"]
    for guid, p in configs:
        name = Path(p).stem
        lines.append(f'Project("{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}") = "{name}", "{p}", "{guid}"')
        lines.append("EndProject")
    lines.append("Global")
    lines.append("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution")
    lines.append("\t\tDebug|Any CPU = Debug|Any CPU")
    lines.append("\t\tRelease|Any CPU = Release|Any CPU")
    lines.append("\tEndGlobalSection")
    lines.append("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution")
    for guid, _ in configs:
        lines.append(f"\t\t{guid}.Debug|Any CPU.ActiveCfg = Debug|Any CPU")
        lines.append(f"\t\t{guid}.Debug|Any CPU.Build.0 = Debug|Any CPU")
        lines.append(f"\t\t{guid}.Release|Any CPU.ActiveCfg = Release|Any CPU")
        lines.append(f"\t\t{guid}.Release|Any CPU.Build.0 = Release|Any CPU")
    lines.append("\tEndGlobalSection")
    lines.append("EndGlobal")
    (ROOT / "EnterpriseTasks.sln").write_text("\n".join(lines) + "\n", encoding="utf-8")


def scripts() -> None:
    write(
        ROOT / "scripts" / "evaluate.ps1",
        """\
        param(
          [Parameter(Mandatory = $true)][string]$Task,
          [Parameter(Mandatory = $true)][ValidateSet('A','B')][string]$Variant
        )

        $root = Split-Path -Parent $PSScriptRoot
        $evalProj = Join-Path $root "Tasks/$Task/$Variant/Evaluation/${Task}${Variant}.Evaluation.csproj"
        if (-not (Test-Path $evalProj)) { throw "Evaluation project not found: $evalProj" }

        Write-Host "Evaluating $Task-$Variant ..."
        dotnet test $evalProj --nologo
        """,
    )
    write(
        ROOT / "scripts" / "pack_participant.ps1",
        """\
        param(
          [Parameter(Mandatory = $true)][string]$Task,
          [Parameter(Mandatory = $true)][ValidateSet('A','B')][string]$Variant,
          [string]$OutDir = "..\\participant_packages"
        )

        $root = Split-Path -Parent $PSScriptRoot
        $destRoot = Join-Path $root $OutDir
        $dest = Join-Path $destRoot "$Task$Variant"
        New-Item -ItemType Directory -Force -Path $dest | Out-Null

        Copy-Item -Recurse -Force (Join-Path $root "Tasks/$Task/$Variant/Contracts") (Join-Path $dest "Contracts")
        Copy-Item -Recurse -Force (Join-Path $root "Tasks/$Task/$Variant/Participant") (Join-Path $dest "Participant")
        Copy-Item -Recurse -Force (Join-Path $root "src/Enterprise.Shared") (Join-Path $dest "Enterprise.Shared")

        $spec = Get-ChildItem (Join-Path $root "task_specs") -Filter "$Task=${Variant}_*" -ErrorAction SilentlyContinue
        # fallback glob
        Get-ChildItem (Join-Path $root "task_specs") -Filter "${Task}_${Variant}_*.md" | ForEach-Object {
          Copy-Item $_.FullName (Join-Path $dest "TASK_SPEC.md") -Force
        }

        Write-Host "Participant package written to $dest"
        Write-Host "NOTE: Evaluation/ intentionally excluded."
        """,
    )


def main() -> None:
    shared_files()
    docs()
    scripts()
    projects: list[str] = [r"src\Enterprise.Shared\Enterprise.Shared.csproj"]

    for pair in PAIRS:
        for vk in ("a", "b"):
            task_spec(pair, vk)
            if pair["kind"] == "crud":
                crud_contracts(pair["id"], vk.upper(), pair[vk])
                crud_participant(pair["id"], vk.upper(), pair[vk])
                crud_evaluation(pair["id"], vk.upper(), pair[vk])
            else:
                generic_service_task(pair, vk)

            variant = vk.upper()
            projects.append(fr"Tasks\{pair['id']}\{variant}\Contracts\{pair['id']}{variant}.Contracts.csproj")
            projects.append(fr"Tasks\{pair['id']}\{variant}\Participant\{pair['id']}{variant}.Participant.csproj")
            projects.append(fr"Tasks\{pair['id']}\{variant}\Evaluation\{pair['id']}{variant}.Evaluation.csproj")

    reference_solution_tp01()
    solution_file(projects)
    print(f"Generated suite at {ROOT}")
    print(f"Projects: {len(projects)}")


if __name__ == "__main__":
    main()
