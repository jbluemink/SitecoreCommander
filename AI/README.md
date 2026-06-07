# AI Documentation Map

This folder contains human-facing playbooks and operational guidance for working with SitecoreCommander.

## Scope Split
- `.github/skills/` contains agent runtime skills (compact, invocable instructions).
- `AI/` contains broader playbooks, templates, and implementation notes for developers and examples.

## Purpose And Audience
- Skills (`.github/skills/`): machine-oriented reference rules for agents and deterministic reasoning.
- Playbooks (`AI/`): human-oriented workflows, templates, and review checklists for contributors.

## Canonical Topics
- API family and auth workflow: `AI/AGENT_API_PLAYBOOK.md`
- Prompt structure for planning/execution: `AI/AGENT_PROMPT_TEMPLATE.md`
- Layout XML and placeholder mechanics for agent reasoning: `.github/skills/sitecore-layout-xml/SKILL.md`
- Placeholder implementation note (short form): `AI/placeholder-implementation-guide.md`
- Dynamic placeholder note (short form): `AI/placeholder-dynamics.md`
- `defaultParameters` encoding checklist: `AI/DEFAULTPARAMETERS-CHECKLIST.md`

## Maintenance Rule
- Keep detailed parsing, rewriting, and validation rules in `.github/skills/sitecore-layout-xml/`.
- Keep AI playbooks concise and link back to the skill when overlap exists.
- Avoid duplicating full rule sets across both locations.

## Validation Notes
- See `AI/DOCUMENTATION-VALIDATION.md` for external validation status, unresolved ambiguities, and requested XML sample format.