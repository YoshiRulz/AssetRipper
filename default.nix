{ system ? builtins.currentSystem
, pkgs ? import (builtins.fetchTarball "https://github.com/YoshiRulz/nixpkgs/archive/arinit-yoshi.tar.gz") { inherit system; } # pending merge of https://github.com/NixOS/nixpkgs/pull/427561
, fetchpatch ? pkgs.fetchpatch
, assetripper ? pkgs.assetripper
}: assetripper.overrideAttrs (finalAttrs: {
	version = "1.3.2";
	src = finalAttrs.src.override (_: {
		tag = "refs/tags/${finalAttrs.version}";
#		tag = null;
#		rev = "2583cf9fcd1f91d5142d391391b2c1aa520f2f64"; # = 01787539d^, where 01787539d reverts the adoption of NuGet CPM because the maintainer's IDE was malfunctioning
#		hash = "sha256-afwpms2Z3mQ7ncDbSa4sZ3rtgjLctgyVo2BIagLwF78=";
	});
	nugetDeps = ./deps.json;
	patches = (finalAttrs.patches or []) ++ [
		(fetchpatch {
			url = "https://github.com/YoshiRulz/AssetRipper/commit/59aa7e7a935f79a6cff685a616d3ce77243ec698.patch";
			hash = "sha256-vbo9eW6tXnG/a0mhIEEILTOESY4jBCpNuHzPWARvPyo=";
		})
	];
})
