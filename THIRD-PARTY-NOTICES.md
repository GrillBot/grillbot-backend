# Third-party notices

GrillBot itself is MIT licensed (see [LICENSE](LICENSE)). This file lists
dependencies that need attribution or that are not plain MIT.

All other NuGet and npm dependencies are under permissive licences (MIT,
Apache-2.0, BSD-3-Clause, PostgreSQL licence). The authoritative list of
.NET packages is `Directory.Packages.props`.

## Attribution

| Component | Licence | Used by |
|---|---|---|
| [Magick.NET](https://github.com/dlemstra/Magick.NET) (ImageMagick) | Apache-2.0 | `ImageProcessingService` |

## AutoMapper: deprecated and planned for removal

AutoMapper is pinned to **14.0.0**, the last release under the MIT licence.
Versions 15 and newer are dual-licensed under the Reciprocal Public License 1.5
or a paid commercial licence, which does not fit an MIT repository.

- **Do not upgrade it.** The pin and its explanation are in
  `Directory.Packages.props`.
- **Known advisory:** 14.0.0 is affected by
  [GHSA-rvv3-g6hj-g44x](https://github.com/advisories/GHSA-rvv3-g6hj-g44x)
  (CVE-2026-32933, high): a stack overflow when mapping deeply nested object
  graphs. The fixed releases (15.1.1 and 16.1.1) are not MIT. The risk is accepted
  because GrillBot maps its own database entities, not attacker-supplied object
  graphs.
- **Plan:** remove AutoMapper entirely and replace it with source-generated
  mapping (for example Mapperly). The code to migrate is in `GrillBot.Data`,
  `GrillBot.App/Startup.cs` (`AddAutoMapper`), the `*MappingProfile` classes and
  the `Actions/Api/V1/*` handlers that use it.

## Assets

Some bundled images (for example the embedded PNG resources in
`src/Services/ImageProcessingService/Resources/`) and favicons were not created by
the GrillBot contributors. The MIT licence covers GrillBot's own code and content
only. It does not grant rights to third-party artwork, which stays with its
respective owners.
